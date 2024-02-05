using System.IO.Abstractions;

namespace Utopia.Core.IO;

/// <summary>
///     文件系统
/// </summary>
public abstract class ResourceLocator : IResourceLocator
{
    public abstract string RootDirectory { get; }

    public string AssetsDirectory => Path.Join(this.RootDirectory, IResourceLocator.DefaultAssetsDirectoryName);

    public string WorldsDirectory => Path.Join(this.RootDirectory, IResourceLocator.DefaultWorldsDirectoryName);

    public string CharactersDirectory => Path.Join(this.RootDirectory, IResourceLocator.DefaultCharactersDirectoryName);

    public string PluginsDirectory => Path.Join(this.RootDirectory, IResourceLocator.DefaultPluginsDirectoryName);

    public string ConfigurationDirectory =>
        Path.Join(this.RootDirectory, IResourceLocator.DefaultConfigurationsDirectoryName);

    public string UtilitiesDirectory => Path.Join(this.RootDirectory, IResourceLocator.DefaultUtilitiesDirectoryName);

    public abstract string? ServerDirectory { get; }
}