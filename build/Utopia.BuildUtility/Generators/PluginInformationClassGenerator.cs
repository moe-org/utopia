// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.Plugin;

namespace Utopia.Tool.Generators;

/// <summary>
/// 生成<see cref="IPluginInformation"/>
/// </summary>
public class PluginInformationClassGenerator
{
    public SourceGenerationTarget Generate(PluginInformation information)
    {
        string shouldWriteTo =
            Path.GetFullPath(Path.Join(information.RootDirectory, information.GenerateCodeDirectoryName,"PluginInformation.cs"));

        return new SourceGenerationTarget { Text = "", ShouldWriteTo = shouldWriteTo };
    }
}
