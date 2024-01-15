// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Plugin;

/// <summary>
/// Warning: <see cref="PluginLifeCycle.Created"/> won't work.
/// That lifecycle won't fire any event and it should has no handler.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LifecycleHandlerAttribute : Attribute
{
    public readonly PluginLifeCycle Lifecycle;

    public LifecycleHandlerAttribute(PluginLifeCycle value)
    {
        Lifecycle = value;
    }
}
