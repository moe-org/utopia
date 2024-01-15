// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Plugin;

/// <summary>
/// 这个Attribute用于构建容器
/// </summary>
[AttributeUsage(AttributeTargets.Method,AllowMultiple = false,Inherited = true)]
public sealed class ContainerBuildAttribute : Attribute
{
    public ContainerBuildAttribute()
    {
    }
}
