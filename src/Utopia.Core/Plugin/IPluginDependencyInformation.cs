// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Plugin;

/// <summary>
/// 插件的依赖信息.这些信息往往在加载插件(所在的dll)之前就需要get到.
/// </summary>
public interface IPluginDependencyInformation
{
    /// <summary>
    /// 插件自己的id
    /// </summary>
    public Guuid Id { get; }

    /// <summary>
    /// 版本号
    /// </summary>
    public SemanticVersioning.Version Version { get; }

    /// <summary>
    /// 依赖的插件的guuid以及版本范围
    /// </summary>
    public  (Guuid, SemanticVersioning.Range)[] Dependences { get; }
}
