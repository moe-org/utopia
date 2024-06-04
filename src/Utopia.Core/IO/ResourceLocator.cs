
namespace Utopia.Core.IO;

/// <summary>
///     文件系统
/// </summary>
public abstract class ResourceLocator : IResourceLocator
{
    public abstract string RootDirectory { get; }

    public string AssetsDirectory => Path.Join(RootDirectory, IResourceLocator.DefaultAssetsDirectoryName).ToString();

    public string WorldsDirectory => Path.Join(RootDirectory, IResourceLocator.DefaultWorldsDirectoryName).ToString();

    public string CharactersDirectory => Path.Join(RootDirectory, IResourceLocator.DefaultCharactersDirectoryName).ToString();

    public string PluginsDirectory => Path.Join(RootDirectory, IResourceLocator.DefaultPluginsDirectoryName).ToString();

    public string ConfigurationDirectory =>
        Path.Join(RootDirectory, IResourceLocator.DefaultConfigurationsDirectoryName).ToString();

    public string UtilitiesDirectory => Path.Join(RootDirectory, IResourceLocator.DefaultUtilitiesDirectoryName).ToString();

    public abstract string? ServerDirectory { get; }

}
