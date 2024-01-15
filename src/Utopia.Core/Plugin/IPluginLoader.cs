// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Autofac;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Utopia.Core.Plugin;

/// <summary>
/// 这个类用于加载插件。即从Type,文件,byte[]等地点加载插件.并且还负责对插件的依赖进行处理.
/// </summary>
public interface IPluginLoader<PluginT> where PluginT:IPluginBase
{
    /// <summary>
    /// 已经加载过的插件.
    /// </summary>
    IEnumerable<PluginT> LoadedPlugins { get; }

    /// <summary>
    /// 激活插件的时候构造触发这个事件.
    /// </summary>
    event Action<ContainerBuilder> ActivatingPlugin;

    /// <summary>
    /// 激活所有<see cref="UnloadedPlugins"/>插件.
    /// </summary>
    void Activate(IEnumerable<IUnloadedPlugin> plugins);
}
