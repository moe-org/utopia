#region

using System.Xml.Serialization;
using Utopia.Core.IO;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

#endregion

namespace Utopia.Core.Plugin;

/// <summary>
///     一个标准的插件查找器.不提供默认插件.
/// </summary>
public class StandardPluginProvider : IPluginProvider
{
    /// <summary>
    ///     从<see cref="IPluginResourceLocator.DefaultPluginManifestFile" />中读取并序列化为<see cref="Manifest" />.
    /// </summary>
    /// <param name="pathToPlugin">要获取<see cref="IPluginResourceLocator.DefaultPluginManifestFile" />文件的目录</param>
    /// <returns>如果清单文件不存在,返回空数组.否则返回查找到的清单文件.</returns>
    public IEnumerable<IUnloadedPlugin> GetPluginAtDirectory(string pathToPlugin)
    {
        var manifest = this.GetManifestFile(pathToPlugin);

        if (manifest is null) return [];

        return this.ProcessManifestFile(manifest, pathToPlugin);
    }

    public IEnumerable<IUnloadedPlugin> GetAllPluginsFrom(string directory)
    {
        var items = Directory.GetFiles(directory,
                                       IPluginResourceLocator.DefaultPluginManifestFile,
                                       SearchOption.AllDirectories);

        List<IUnloadedPlugin> plugins = [];

        foreach (var item in items)
        {
            plugins.AddRange(GetPluginAtDirectory(Path.GetFullPath(item))
                                                  ?? throw new InvalidOperationException(
                                                      $"failed to get directory name for plugin at {item}"));
        }

        return plugins;
    }

    /// <summary>
    ///     从插件文件夹中获取清单文件.
    /// </summary>
    /// <returns>如果返回null,则代表清单文件不存在.否则返回获取到的清单文件.</returns>
    protected Manifest? GetManifestFile(string directory)
    {
        // 查找清单文件
        var manifestFile = Path.Join(directory, IPluginResourceLocator.DefaultPluginManifestFile);

        if (!File.Exists(manifestFile)) return null;

        // 读取
        XmlSerializer serializer = new(typeof(Manifest));

        using var fs = File.Open(manifestFile, FileMode.Open, FileAccess.Read);
        var manifest = serializer.Deserialize<Manifest>(fs);

        return manifest;
    }

    /// <summary>
    ///     从清单文件获取要加载的插件.
    /// </summary>
    /// <param name="manifest"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    protected IEnumerable<IUnloadedPlugin> ProcessManifestFile(Manifest manifest, string dir)
    {
        List<IUnloadedPlugin> found = new();

        foreach (var item in manifest.Plugins)
        {
            List<(Guuid, Range)> deps = [];
            foreach (var dependency in item.Dependencies)
                deps.Add(new ValueTuple<Guuid, Range>(
                    dependency.Id.Guuid,
                    Range.Parse(dependency.RequiredVersionRange)
                ));

            PluginDependencyInformation information = new()
            {
                Version = Version.Parse(item.Version),
                Id = item.Id.Guuid,
                Dependencies = deps.ToArray()
            };

            found.Add(new UnloadedAssemblyPlugin
            {
                Info = information,
                TypeName = [item.TypeName],
                Assemblies =
                    item.Assemblies.Select(assembly => Path.GetFullPath(Path.Join(dir, assembly))).ToArray()
            });
        }

        return found;
    }
}
