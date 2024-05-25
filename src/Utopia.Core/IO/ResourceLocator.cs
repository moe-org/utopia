

using Zio;

namespace Utopia.Core.IO;

/// <summary>
///     文件系统
/// </summary>
public abstract class ResourceLocator : IResourceLocator
{
    public abstract IFileSystem FileSystem { get; }

    public abstract string RootDirectory { get; }

    public string AssetsDirectory => UPath.Combine(RootDirectory, IResourceLocator.DefaultAssetsDirectoryName).ToString();

    public string WorldsDirectory => UPath.Combine(RootDirectory, IResourceLocator.DefaultWorldsDirectoryName).ToString();

    public string CharactersDirectory => UPath.Combine(RootDirectory, IResourceLocator.DefaultCharactersDirectoryName).ToString();

    public string PluginsDirectory => UPath.Combine(RootDirectory, IResourceLocator.DefaultPluginsDirectoryName).ToString();

    public string ConfigurationDirectory =>
        UPath.Combine(RootDirectory, IResourceLocator.DefaultConfigurationsDirectoryName).ToString();

    public string UtilitiesDirectory => UPath.Combine(RootDirectory, IResourceLocator.DefaultUtilitiesDirectoryName).ToString();

    public abstract string? ServerDirectory { get; }

}
