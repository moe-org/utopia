// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Net.Mime;

namespace Utopia.Tool.Generators;

/// <summary>
/// 复用模板的生成器
/// </summary>
public class TemplateGenerator
{

    /// <summary>
    /// 生成
    /// </summary>
    public SourceGenerationTarget Generate(PluginInformation information,string template,Dictionary<string,string> data)
    {
        foreach (var key in data.Keys)
        {
            template = template.Replace('$' + key + '$', data[key]);
        }

        string shouldWriteTo =
            Path.GetFullPath(Path.Join(information.RootDirectory, information.GenerateCodeDirectoryName, Utility.Sha256(template) + ".cs"));

        return new SourceGenerationTarget { Text = template, ShouldWriteTo = shouldWriteTo };
    }
}
