// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Plugin;

/// <summary>
/// The lifecycle of a plugin.
/// </summary>
public enum PluginLifeCycle
{
    /// <summary>
    /// 插件的初始状态.
    /// </summary>
    Activated,
    /// <summary>
    /// 插件已经执行了Unload操作.
    /// </summary>
    Deactivated,
}

