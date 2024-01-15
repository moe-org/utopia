// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Plugin;

/// <summary>
/// 这个接口用于提供插件加载信息,供<see cref="IPluginLoader{PluginT}"/>使用.
/// </summary>
public interface IPluginProvider
{
    /// <summary>
    /// 从文件夹获取应该加载的插件
    /// </summary>
    IEnumerable<IUnloadedPlugin> GetPlugin(string pathToPlugin);
}
