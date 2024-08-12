﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.ProjectSystem;
using Microsoft.AspNetCore.Razor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.AspNetCore.Razor.Test.Common.ProjectSystem;

internal class TestDocumentSnapshot : DocumentSnapshot
{
    private RazorCodeDocument? _codeDocument;

    public static TestDocumentSnapshot Create(string filePath)
        => Create(filePath, string.Empty);

    public static TestDocumentSnapshot Create(string filePath, VersionStamp version)
        => Create(filePath, string.Empty, version);

    public static TestDocumentSnapshot Create(string filePath, string text, int version = 0)
        => Create(filePath, text, VersionStamp.Default, numericVersion: version);

    public static TestDocumentSnapshot Create(string filePath, string text, VersionStamp version, ProjectWorkspaceState? projectWorkspaceState = null, int numericVersion = 0)
        => Create(filePath, text, version, TestProjectSnapshot.Create(filePath + ".csproj", projectWorkspaceState), numericVersion);

    public static TestDocumentSnapshot Create(string filePath, string text, VersionStamp version, TestProjectSnapshot projectSnapshot, int numericVersion)
    {
        var targetPath = Path.GetDirectoryName(projectSnapshot.FilePath) is string projectDirectory && filePath.StartsWith(projectDirectory)
            ? filePath[projectDirectory.Length..]
            : filePath;

        var hostDocument = new HostDocument(filePath, targetPath);
        var sourceText = SourceText.From(text);
        var documentState = new DocumentState(
            hostDocument,
            SourceText.From(text),
            version,
            numericVersion,
            () => Task.FromResult(TextAndVersion.Create(sourceText, version)));
        var testDocument = new TestDocumentSnapshot(projectSnapshot, documentState);

        return testDocument;
    }

    internal static TestDocumentSnapshot Create(ProjectSnapshot projectSnapshot, string filePath, string text = "", VersionStamp? version = null)
    {
        version ??= VersionStamp.Default;

        var targetPath = FilePathNormalizer.Normalize(filePath);
        var projectDirectory = FilePathNormalizer.GetNormalizedDirectoryName(projectSnapshot.FilePath);
        if (targetPath.StartsWith(projectDirectory))
        {
            targetPath = targetPath[projectDirectory.Length..];
        }

        var hostDocument = new HostDocument(filePath, targetPath);
        var sourceText = SourceText.From(text);
        var documentState = new DocumentState(
            hostDocument,
            SourceText.From(text),
            version,
            1,
            () => Task.FromResult(TextAndVersion.Create(sourceText, version.Value)));
        var testDocument = new TestDocumentSnapshot(projectSnapshot, documentState);

        return testDocument;
    }

    public TestDocumentSnapshot(ProjectSnapshot projectSnapshot, DocumentState documentState)
        : base(projectSnapshot, documentState)
    {
    }

    public HostDocument HostDocument => State.HostDocument;

    public override Task<RazorCodeDocument> GetGeneratedOutputAsync()
    {
        if (_codeDocument is null)
        {
            throw new ArgumentNullException(nameof(_codeDocument));
        }

        return Task.FromResult(_codeDocument);
    }

    public override bool TryGetGeneratedOutput(out RazorCodeDocument result)
    {
        if (_codeDocument is null)
        {
            throw new InvalidOperationException($"You must call {nameof(With)} to set the code document for this document snapshot.");
        }

        result = _codeDocument;
        return true;
    }

    public TestDocumentSnapshot With(RazorCodeDocument codeDocument)
    {
        if (codeDocument is null)
        {
            throw new ArgumentNullException(nameof(codeDocument));
        }

        _codeDocument = codeDocument;
        return this;
    }
}
