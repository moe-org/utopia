// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.IO;

/// <summary>
///     Like <see cref="IResourceLocator" />,but it is used for plugins in runtime.
/// </summary>
public interface IPluginResourceLocator
{
    /// <summary>
    ///     默认的清单文件路径.清单文件的类型见<see cref="Plugin.Manifest" />
    /// </summary>
    public const string DefaultPluginManifestFile = "Manifest.xml";

    /// <summary>
    ///     The directory path of the plugin.
    /// </summary>
    string RootDirectory { get; }

    /// <summary>
    ///     全局配置文件目录。
    /// </summary>
    string GlobalConfigurationDirectory { get; }
}
