#region

using System.IO.Abstractions;
using Utopia.Core.Plugin;

#endregion

namespace Utopia.Core.IO;

/// <summary>
///     The file system
/// </summary>
public interface IResourceLocator
{
    public const string DefaultAssetsDirectoryName = "Assets";

    public const string DefaultPluginsDirectoryName = "Plugins";

    public const string DefaultWorldsDirectoryName = "Worlds";

    public const string DefaultCharactersDirectoryName = "Characters";

    public const string DefaultConfigurationsDirectoryName = "Configurations";

    public const string DefaultServerDirectoryName = "Server";

    public const string DefaultUtilitiesDirectoryName = "Utilities";
    
    IFileSystem FileSystem { get; }

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
        FileSystem.Directory.CreateDirectory(this.RootDirectory);
        FileSystem.Directory.CreateDirectory(this.AssetsDirectory);
        FileSystem.Directory.CreateDirectory(this.WorldsDirectory);
        FileSystem.Directory.CreateDirectory(this.CharactersDirectory);
        FileSystem.Directory.CreateDirectory(this.PluginsDirectory);
        FileSystem.Directory.CreateDirectory(this.ConfigurationDirectory);
        FileSystem.Directory.CreateDirectory(this.UtilitiesDirectory);
        if (this.ServerDirectory != null) 
            FileSystem.Directory.CreateDirectory(this.ServerDirectory);
    }

    string GetConfigurationDirectoryOfPlugin(IPluginInformation plugin)
    {
        var path = FileSystem.Path.Join(this.ConfigurationDirectory, plugin.Id.ToCsIdentifier());
        FileSystem.Directory.CreateDirectory(path);
        return path;
    }
}