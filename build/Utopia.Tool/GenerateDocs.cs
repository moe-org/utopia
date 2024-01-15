// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Markdig;
using Docfx;
using Docfx.Dotnet;

namespace Utopia.Tool;

/// <summary>
/// 生成文档
/// </summary>
public static class GenerateDocs
{
    public static void Generate(string configFile)
    {
        var options = new BuildOptions
        {
            // Enable custom markdown extensions here
            ConfigureMarkdig = pipeline => pipeline.UseCitations(),
        };

        DotnetApiCatalog.GenerateManagedReferenceYamlFiles(configFile).Wait();
        Docset.Build(configFile, options).Wait();
    }
}
