
using System.IO.Abstractions;

namespace Utopia.Core.IO;

/// <summary>
///     文件系统
/// </summary>
public abstract class ResourceLocator : IResourceLocator
{
    public abstract IFileSystem FileSystem { get; }

    public abstract string RootDirectory { get; }

    public string AssetsDirectory => FileSystem.Path.Join(RootDirectory, IResourceLocator.DefaultAssetsDirectoryName);

    public string WorldsDirectory => FileSystem.Path.Join(RootDirectory, IResourceLocator.DefaultWorldsDirectoryName);

    public string CharactersDirectory => FileSystem.Path.Join(RootDirectory, IResourceLocator.DefaultCharactersDirectoryName);

    public string PluginsDirectory => FileSystem.Path.Join(RootDirectory, IResourceLocator.DefaultPluginsDirectoryName);

    public string ConfigurationDirectory =>
        FileSystem.Path.Join(RootDirectory, IResourceLocator.DefaultConfigurationsDirectoryName);

    public string UtilitiesDirectory => FileSystem.Path.Join(RootDirectory, IResourceLocator.DefaultUtilitiesDirectoryName);

    public abstract string? ServerDirectory { get; }

}
