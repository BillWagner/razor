﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;
using Microsoft.AspNetCore.Razor.LanguageServer.ProjectSystem;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.DocumentMapping;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;
using Microsoft.CodeAnalysis.Razor.Protocol;
using Microsoft.CodeAnalysis.Razor.Tooltip;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor.Razor;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Text.Adornments;

namespace Microsoft.AspNetCore.Razor.LanguageServer.Hover;

internal sealed partial class HoverService(
    IProjectSnapshotManager projectManager,
    IDocumentMappingService documentMappingService,
    IClientCapabilitiesService clientCapabilitiesService) : IHoverService
{
    private readonly IProjectSnapshotManager _projectManager = projectManager;
    private readonly IDocumentMappingService _documentMappingService = documentMappingService;
    private readonly IClientCapabilitiesService _clientCapabilitiesService = clientCapabilitiesService;

    public async Task<VSInternalHover?> GetRazorHoverInfoAsync(DocumentContext documentContext, DocumentPositionInfo positionInfo, CancellationToken cancellationToken)
    {
        // HTML can still sometimes be handled by razor. For example hovering over
        // a component tag like <Counter /> will still be in an html context
        if (positionInfo.LanguageKind == RazorLanguageKind.CSharp)
        {
            return null;
        }

        var codeDocument = await documentContext.GetCodeDocumentAsync(cancellationToken).ConfigureAwait(false);

        // Sometimes what looks like a html attribute can actually map to C#, in which case its better to let Roslyn try to handle this.
        // We can only do this if we're in single server mode though, otherwise we won't be delegating to Roslyn at all
        if (_documentMappingService.TryMapToGeneratedDocumentPosition(codeDocument.GetCSharpDocument(), positionInfo.HostDocumentIndex, out _, out _))
        {
            return null;
        }

        var options = HoverDisplayOptions.From(_clientCapabilitiesService.ClientCapabilities);

        return await GetHoverInfoAsync(
            documentContext.FilePath, codeDocument, positionInfo.HostDocumentIndex, options, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VSInternalHover?> TranslateDelegatedResponseAsync(VSInternalHover? response, DocumentContext documentContext, DocumentPositionInfo positionInfo, CancellationToken cancellationToken)
    {
        if (response?.Range is null)
        {
            return response;
        }

        var codeDocument = await documentContext.GetCodeDocumentAsync(cancellationToken).ConfigureAwait(false);

        // If we don't include the originally requested position in our response, the client may not show it, so we extend the range to ensure it is in there.
        // eg for hovering at @bind-Value:af$$ter, we want to show people the hover for the Value property, so Roslyn will return to us the range for just the
        // portion of the attribute that says "Value".
        if (RazorSyntaxFacts.TryGetFullAttributeNameSpan(codeDocument, positionInfo.HostDocumentIndex, out var originalAttributeRange))
        {
            var sourceText = await documentContext.GetSourceTextAsync(cancellationToken).ConfigureAwait(false);
            response.Range = sourceText.GetRange(originalAttributeRange);
        }
        else if (positionInfo.LanguageKind == RazorLanguageKind.CSharp)
        {
            if (_documentMappingService.TryMapToHostDocumentRange(codeDocument.GetCSharpDocument(), response.Range, out var projectedRange))
            {
                response.Range = projectedRange;
            }
            else
            {
                // We couldn't remap the range back from Roslyn, but we have to do something with it, because it definitely won't
                // be correct, and if the Razor document is small, will be completely outside the valid range for the file, which
                // would cause the client to error.
                // Returning null here will still show the hover, just there won't be any extra visual indication, like
                // a background color, applied by the client.
                response.Range = null;
            }
        }

        return response;
    }

    private async Task<VSInternalHover?> GetHoverInfoAsync(
        string documentFilePath,
        RazorCodeDocument codeDocument,
        int absoluteIndex,
        HoverDisplayOptions options,
        CancellationToken cancellationToken)
    {
        var syntaxTree = codeDocument.GetSyntaxTree();

        var owner = syntaxTree.Root.FindInnermostNode(absoluteIndex);
        if (owner is null)
        {
            Debug.Fail("Owner should never be null.");
            return null;
        }

        // For cases where the point in the middle of an attribute,
        // such as <any tes$$t=""></any>
        // the node desired is the *AttributeSyntax
        if (owner.Kind is SyntaxKind.MarkupTextLiteral)
        {
            owner = owner.Parent;
        }

        var tagHelperDocumentContext = codeDocument.GetTagHelperContext();

        // We want to find the parent tag, but looking up ancestors in the tree can find other things,
        // for example when hovering over a start tag, the first ancestor is actually the element it
        // belongs to, or in other words, the exact same tag! To work around this we just make sure we
        // only check nodes that are at a different location in the file.
        var ownerStart = owner.SpanStart;

        if (HtmlFacts.TryGetElementInfo(owner, out var containingTagNameToken, out var attributes, closingForwardSlashOrCloseAngleToken: out _) &&
            containingTagNameToken.Span.IntersectsWith(absoluteIndex))
        {
            if (owner is MarkupStartTagSyntax or MarkupEndTagSyntax &&
                containingTagNameToken.Content.Equals(SyntaxConstants.TextTagName, StringComparison.OrdinalIgnoreCase))
            {
                // It's possible for there to be a <Text> component that is in scope, and would be found by the GetTagHelperBinding
                // call below, but a text tag, regardless of casing, inside C# code, is always just a text tag, not a component.
                return null;
            }

            // Hovering over HTML tag name
            var ancestors = owner.Ancestors().Where(n => n.SpanStart != ownerStart);
            var (parentTag, parentIsTagHelper) = TagHelperFacts.GetNearestAncestorTagInfo(ancestors);
            var stringifiedAttributes = TagHelperFacts.StringifyAttributes(attributes);
            var binding = TagHelperFacts.GetTagHelperBinding(
                tagHelperDocumentContext,
                containingTagNameToken.Content,
                stringifiedAttributes,
                parentTag: parentTag,
                parentIsTagHelper: parentIsTagHelper);

            if (binding is null)
            {
                // No matching tagHelpers, it's just HTML
                return null;
            }
            else if (binding.IsAttributeMatch)
            {
                // Hovered over a HTML tag name but the binding matches an attribute
                return null;
            }
            else
            {
                Debug.Assert(binding.Descriptors.Any());

                var span = containingTagNameToken.GetLinePositionSpan(codeDocument.Source);

                return await ElementInfoToHoverAsync(documentFilePath, binding.Descriptors, span, options, cancellationToken).ConfigureAwait(false);
            }
        }

        if (HtmlFacts.TryGetAttributeInfo(owner, out containingTagNameToken, out _, out var selectedAttributeName, out var selectedAttributeNameLocation, out attributes) &&
            selectedAttributeNameLocation?.IntersectsWith(absoluteIndex) == true)
        {
            // When finding parents for attributes, we make sure to find the parent of the containing tag, otherwise these methods
            // would return the parent of the attribute, which is not helpful, as its just going to be the containing element
            var containingTag = containingTagNameToken.Parent;
            var ancestors = containingTag.Ancestors().Where<SyntaxNode>(n => n.SpanStart != containingTag.SpanStart);
            var (parentTag, parentIsTagHelper) = TagHelperFacts.GetNearestAncestorTagInfo(ancestors);

            // Hovering over HTML attribute name
            var stringifiedAttributes = TagHelperFacts.StringifyAttributes(attributes);

            var binding = TagHelperFacts.GetTagHelperBinding(
                tagHelperDocumentContext,
                containingTagNameToken.Content,
                stringifiedAttributes,
                parentTag: parentTag,
                parentIsTagHelper: parentIsTagHelper);

            if (binding is null)
            {
                // No matching TagHelpers, it's just HTML
                return null;
            }
            else
            {
                Debug.Assert(binding.Descriptors.Any());
                var tagHelperAttributes = TagHelperFacts.GetBoundTagHelperAttributes(
                    tagHelperDocumentContext,
                    selectedAttributeName.AssumeNotNull(),
                    binding);

                // Grab the first attribute that we find that intersects with this location. That way if there are multiple attributes side-by-side aka hovering over:
                //      <input checked| minimized />
                // Then we take the left most attribute (attributes are returned in source order).
                var attribute = attributes.First(a => a.Span.IntersectsWith(absoluteIndex));
                if (attribute is MarkupTagHelperAttributeSyntax thAttributeSyntax)
                {
                    attribute = thAttributeSyntax.Name;
                }
                else if (attribute is MarkupMinimizedTagHelperAttributeSyntax thMinimizedAttribute)
                {
                    attribute = thMinimizedAttribute.Name;
                }
                else if (attribute is MarkupTagHelperDirectiveAttributeSyntax directiveAttribute)
                {
                    attribute = directiveAttribute.Name;
                }
                else if (attribute is MarkupMinimizedTagHelperDirectiveAttributeSyntax miniDirectiveAttribute)
                {
                    attribute = miniDirectiveAttribute;
                }

                var attributeName = attribute.GetContent();
                var span = attribute.GetLinePositionSpan(codeDocument.Source);

                // Include the @ in the range
                switch (attribute.Parent.Kind)
                {
                    case SyntaxKind.MarkupTagHelperDirectiveAttribute:
                        var directiveAttribute = (MarkupTagHelperDirectiveAttributeSyntax)attribute.Parent;
                        span = span.WithStart(start => start.WithCharacter(ch => ch - directiveAttribute.Transition.FullWidth));
                        attributeName = "@" + attributeName;
                        break;

                    case SyntaxKind.MarkupMinimizedTagHelperDirectiveAttribute:
                        var minimizedAttribute = (MarkupMinimizedTagHelperDirectiveAttributeSyntax)containingTag;
                        span = span.WithStart(start => start.WithCharacter(ch => ch - minimizedAttribute.Transition.FullWidth));
                        attributeName = "@" + attributeName;
                        break;
                }

                return AttributeInfoToHover(tagHelperAttributes, attributeName, span, options);
            }
        }

        return null;
    }

    private static VSInternalHover? AttributeInfoToHover(
        ImmutableArray<BoundAttributeDescriptor> boundAttributes,
        string attributeName,
        LinePositionSpan span,
        HoverDisplayOptions options)
    {
        var descriptionInfos = boundAttributes.SelectAsArray(boundAttribute =>
        {
            var isIndexer = TagHelperMatchingConventions.SatisfiesBoundAttributeIndexer(boundAttribute, attributeName.AsSpan());
            return BoundAttributeDescriptionInfo.From(boundAttribute, isIndexer);
        });

        var attrDescriptionInfo = new AggregateBoundAttributeDescription(descriptionInfos);

        if (options.SupportsVisualStudioExtensions &&
            ClassifiedTagHelperTooltipFactory.TryCreateTooltip(attrDescriptionInfo, out ContainerElement? classifiedTextElement))
        {
            var vsHover = new VSInternalHover
            {
                Contents = Array.Empty<SumType<string, MarkedString>>(),
                Range = span.ToRange(),
                RawContent = classifiedTextElement,
            };

            return vsHover;
        }
        else
        {
            if (!MarkupTagHelperTooltipFactory.TryCreateTooltip(attrDescriptionInfo, options.MarkupKind, out var vsMarkupContent))
            {
                return null;
            }

            var markupContent = new MarkupContent()
            {
                Value = vsMarkupContent.Value,
                Kind = vsMarkupContent.Kind,
            };

            var hover = new VSInternalHover
            {
                Contents = markupContent,
                Range = span.ToRange(),
            };

            return hover;
        }
    }

    private async Task<VSInternalHover?> ElementInfoToHoverAsync(
        string documentFilePath,
        IEnumerable<TagHelperDescriptor> descriptors,
        LinePositionSpan span,
        HoverDisplayOptions options,
        CancellationToken cancellationToken)
    {
        var descriptionInfos = descriptors.SelectAsArray(BoundElementDescriptionInfo.From);
        var elementDescriptionInfo = new AggregateBoundElementDescription(descriptionInfos);

        if (options.SupportsVisualStudioExtensions)
        {
            var classifiedTextElement = await ClassifiedTagHelperTooltipFactory
                .TryCreateTooltipContainerAsync(documentFilePath, elementDescriptionInfo, _projectManager.GetQueryOperations(), cancellationToken)
                .ConfigureAwait(false);

            if (classifiedTextElement is not null)
            {
                var vsHover = new VSInternalHover
                {
                    Contents = Array.Empty<SumType<string, MarkedString>>(),
                    Range = span.ToRange(),
                    RawContent = classifiedTextElement,
                };

                return vsHover;
            }
        }

        var vsMarkupContent = await MarkupTagHelperTooltipFactory
            .TryCreateTooltipAsync(documentFilePath, elementDescriptionInfo, _projectManager.GetQueryOperations(), options.MarkupKind, cancellationToken)
            .ConfigureAwait(false);

        if (vsMarkupContent is null)
        {
            return null;
        }

        var markupContent = new MarkupContent()
        {
            Value = vsMarkupContent.Value,
            Kind = vsMarkupContent.Kind,
        };

        var hover = new VSInternalHover
        {
            Contents = markupContent,
            Range = span.ToRange()
        };

        return hover;
    }
}
