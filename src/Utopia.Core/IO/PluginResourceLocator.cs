#region

using Utopia.Core.Plugin;

#endregion

namespace Utopia.Core.IO;

public class PluginResourceLocator(
    IResourceLocator resourceLocator,
    IPluginInformation information,
    string rootDirectory) : IPluginResourceLocator
{
    
    public IResourceLocator GameRootResourceLocator { get; init; } = resourceLocator;

    public string RootDirectory =>
        Path.GetFullPath(rootDirectory);

    public string GlobalConfigurationDirectory =>
        this.GameRootResourceLocator.GetConfigurationDirectoryOfPlugin(information);
}
