// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.IO;

/// <summary>
/// 文件系统
/// </summary>
public abstract class FileSystem : IFileSystem
{
    public abstract string RootDirectory { get; }

    public string AssetsDirectory => Path.Join(RootDirectory, IFileSystem.DefaultAssetsDirectoryName);

    public string WorldsDirectory => Path.Join(RootDirectory, IFileSystem.DefaultWorldsDirectoryName);

    public string CharactersDirectory => Path.Join(RootDirectory, IFileSystem.DefaultCharactersDirectoryName);

    public string PluginsDirectory => Path.Join(RootDirectory, IFileSystem.DefaultPluginsDirectoryName);

    public string ConfigurationDirectory => Path.Join(RootDirectory, IFileSystem.DefaultConfigurationsDirectoryName);

    public string UtilitiesDirectory => Path.Join(RootDirectory, IFileSystem.DefaultUtilitiesDirectoryName);

    public abstract string? ServerDirectory { get; }
}
