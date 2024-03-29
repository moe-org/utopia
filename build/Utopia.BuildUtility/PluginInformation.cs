// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Tool;

/// <summary>
/// develop-time的插件信息。
/// </summary>
public class PluginInformation(string version, string root, params string[] nodes)
    : ManifestInformation(version, root, nodes)
{
    public required string RootDirectory { get; set; }

    public required bool IsServer { get; set; }

    public required bool IsClient { get; set; }

    public required string RootNamespace { get; set; }

    public required string License { get; set; }

    public required string Homepage { get; set; }

    /// <summary>
    /// 用于保存生成的源文件的路径。
    /// 相对于<see cref="RootDirectory"/>的路径。
    /// </summary>
    public string GenerateCodeDirectoryName { get; set; }= "Generated/";
}
