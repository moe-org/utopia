#region

using System.IO.Abstractions;
using Utopia.Core.Plugin;

#endregion

namespace Utopia.Core.IO;

public class PluginResourceLocator(
    IResourceLocator resourceLocator,
    IPluginInformation information,
    string rootDirectory,
    IFileSystem fileSystem) : IPluginResourceLocator
{

    public IResourceLocator GameRootResourceLocator { get; init; } = resourceLocator;

    public string RootDirectory =>
        fileSystem.Path.GetFullPath(rootDirectory);

    public string GlobalConfigurationDirectory =>
        this.GameRootResourceLocator.GetConfigurationDirectoryOfPlugin(information);
}
