﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.VisualStudio.Razor.ProjectSystem;

internal sealed record class FallbackHostProject : HostProject
{
    public FallbackHostProject(
        string filePath,
        string intermediateOutputPath,
        RazorConfiguration configuration,
        string? rootNamespace,
        string displayName)
        : base(filePath, intermediateOutputPath, configuration, rootNamespace, displayName)
    {
    }
}
