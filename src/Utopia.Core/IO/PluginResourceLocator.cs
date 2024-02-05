#region

using System.IO.Abstractions;
using Utopia.Core.Plugin;

#endregion

namespace Utopia.Core.IO;

public class PluginResourceLocator(
    IFileSystem fileSystem,
    IResourceLocator resourceLocator,
    IPluginInformation information,
    string rootDirectory) : IPluginResourceLocator
{

    public IFileSystem FileSystem { get; init; } = fileSystem;
    
    public IResourceLocator GameRootResourceLocator { get; init; } = resourceLocator;

    public string RootDirectory =>
        FileSystem.Path.GetFullPath(rootDirectory);

    public string GlobalConfigurationDirectory =>
        this.GameRootResourceLocator.GetConfigurationDirectoryOfPlugin(information);
}