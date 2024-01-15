// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Utopia.Core.IO;

namespace Utopia.Core.Plugin;

/// <summary>
/// 一个标准的插件查找器.不提供默认插件.
/// </summary>
public class StandardPluginProvider : IPluginProvider
{
    /// <summary>
    /// 从插件文件夹中获取清单文件.
    /// </summary>
    /// <returns>如果返回null,则代表清单文件不存在.否则返回获取到的清单文件.</returns>
    protected Manifest? GetManifestFile(string directory)
    {
        // 查找清单文件
        var manifestFile = Path.GetFullPath(Path.Join(directory, IPluginFileSystem.DefaultPluginManifestFile));

        if (!File.Exists(manifestFile))
        {
            return null;
        }

        // 读取
        XmlSerializer serializer = new(typeof(Manifest));

        using var fs = File.OpenRead(manifestFile);
        var manifest = serializer.Deserialize<Manifest>(fs);

        return manifest;
    }

    /// <summary>
    /// 从清单文件获取要加载的插件.
    /// </summary>
    /// <param name="manifest"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    protected IEnumerable<IUnloadedPlugin> ProcessManifestFile(Manifest manifest,string dir)
    {
        List<IUnloadedPlugin> found = new();

        foreach (var item in manifest.Plugins)
        {
            List<(Guuid, SemanticVersioning.Range)> deps = [];
            foreach (var dependency in item.Dependencies)
            {
                deps.Add(new(
                    dependency.PluginId.Guuid,
                    SemanticVersioning.Range.Parse(dependency.RequiredVersionRange)
                ));
            }

            PluginDependencyInformation information = new()
            {
                Version = SemanticVersioning.Version.Parse(item.PluginVersion),
                Id = item.PluginId.Guuid,
                Dependences = deps.ToArray()
            };

            found.Add(new UnloadedAssemblyPlugin()
            {
                Info = information,
                TypeName = [item.TypeName],
                Assemblies =
                    item.AssemblyLoad.Select((assembly) => Path.GetFullPath(Path.Join(dir, assembly))).ToArray()
            });
        }

        return found;
    }

    /// <summary>
    /// 从<see cref="IPluginFileSystem.DefaultPluginManifestFile"/>中读取并序列化为<see cref="Manifest"/>.
    /// </summary>
    /// <param name="pathToPlugin">要获取<see cref="IPluginFileSystem.DefaultPluginManifestFile"/>文件的目录</param>
    /// <returns>如果清单文件不存在,返回空数组.否则返回查找到的清单文件.</returns>
    public IEnumerable<IUnloadedPlugin> GetPlugin(string pathToPlugin)
    {
        var manifest = GetManifestFile(pathToPlugin);

        if(manifest is null)
        {
            return [];
        }

        return ProcessManifestFile(manifest,pathToPlugin);
    }
}
