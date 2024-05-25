#region

using Utopia.Core.Plugin;
using Zio;

#endregion

namespace Utopia.Core.IO;

/// <summary>
///     The file locator
/// </summary>
public interface IResourceLocator
{
    public IFileSystem FileSystem { get; }

    public const string DefaultAssetsDirectoryName = "Assets";

    public const string DefaultPluginsDirectoryName = "Plugins";

    public const string DefaultWorldsDirectoryName = "Worlds";

    public const string DefaultCharactersDirectoryName = "Characters";

    public const string DefaultConfigurationsDirectoryName = "Configurations";

    public const string DefaultServerDirectoryName = "Server";

    public const string DefaultUtilitiesDirectoryName = "Utilities";

    /// <summary>
    ///     游戏/server所在位置的根目录
    /// </summary>
    string RootDirectory { get; }

    /// <summary>
    ///     游戏的资产目录
    /// </summary>
    string AssetsDirectory { get; }

    /// <summary>
    ///     游戏的世界目录
    /// </summary>
    string WorldsDirectory { get; }

    /// <summary>
    ///     游戏的角色目录
    /// </summary>
    string CharactersDirectory { get; }

    /// <summary>
    ///     游戏的插件目录,where the plugins are packed
    /// </summary>
    string PluginsDirectory { get; }

    /// <summary>
    ///     游戏的配置文件目录
    /// </summary>
    string ConfigurationDirectory { get; }

    /// <summary>
    ///     对于游戏的客户端，这是服务器的<see cref="RootDirectory" />。
    ///     对于服务器，返回null。
    /// </summary>
    string? ServerDirectory { get; }

    /// <summary>
    ///     游戏的工具目录。用于存放一些其他东西。
    /// </summary>
    string UtilitiesDirectory { get; }

    /// <summary>
    ///     对于不存在的目录，则创建一个
    /// </summary>
    void CreateIfNotExist()
    {
        FileSystem.CreateDirectory(RootDirectory);
        FileSystem.CreateDirectory(AssetsDirectory);
        FileSystem.CreateDirectory(WorldsDirectory);
        FileSystem.CreateDirectory(CharactersDirectory);
        FileSystem.CreateDirectory(PluginsDirectory);
        FileSystem.CreateDirectory(ConfigurationDirectory);
        FileSystem.CreateDirectory(UtilitiesDirectory);
        if (ServerDirectory != null)
            FileSystem.CreateDirectory(ServerDirectory);
    }

    string GetConfigurationDirectoryOfPlugin(IPluginInformation plugin)
    {
        var path = UPath.Combine(ConfigurationDirectory, plugin.Id.ToCsIdentifier());
        FileSystem.CreateDirectory(path);
        return path.ToString();
    }
}
