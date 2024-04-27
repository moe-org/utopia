// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Reflection;
using Godot;

namespace Utopia.Godot.Utility;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GodotNodeBindAttribute : Attribute
{
    /// <summary>
    /// 如果是Empty,那么则将field或者property的名字作为NodeName.
    /// 使用<see cref="Node.FindChild(string, bool, bool)"/>进行搜索。
    /// </summary>
    public string NodeName { get; init; } = string.Empty;

    public GodotNodeBindAttribute(string nodeName = "")
    {
        ArgumentNullException.ThrowIfNull(nodeName);
        NodeName = nodeName;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GodotResourceBindAttribute : Attribute
{
    /// <summary>
    /// 如果是Empty,那么则将field或者property的名字作为ResourceName。
    /// 使用<see cref="ResourceLoader.Load(string, string, ResourceLoader.CacheMode)"/>获取资源。
    /// </summary>
    public string ResourcePath { get; init; } = string.Empty;

    public GodotResourceBindAttribute(string resourcePath = "")
    {
        ArgumentNullException.ThrowIfNull(resourcePath);
        ResourcePath = resourcePath;
    }
}

public static class GodotBinder
{
    public static void BindNode(object target, Node source, BindingFlags flags)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        Type type = target.GetType();

        foreach (var item in type.GetProperties(flags))
        {
            GodotNodeBindAttribute? bind = item.GetCustomAttribute<GodotNodeBindAttribute>();

            if (bind != null)
            {
                item.SetValue(target, source.FindChild(bind.NodeName == string.Empty ?
                    item.Name : bind.NodeName));
            }
        }
    }

    public static void BindResource(object target, Node source, BindingFlags flags)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        Type type = target.GetType();

        foreach (PropertyInfo item in type.GetProperties(flags))
        {
            GodotResourceBindAttribute? resource = item.GetCustomAttribute<GodotResourceBindAttribute>();

            if (resource != null)
            {
                Resource ss = ResourceLoader.Load(resource.ResourcePath == string.Empty ?
                    item.Name : resource.ResourcePath);
                item.SetValue(target, ss);
            }
        }
    }

    /// <summary>
    /// A helper function
    /// </summary>
    public static void BindBoth(Node target)
    {
        BindNode(target, target, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        BindResource(target, target, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }
}
