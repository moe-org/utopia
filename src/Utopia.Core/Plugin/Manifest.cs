// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utopia.Core.Plugin;

public class PluginDependency
{
    [XmlElement]
    public XmlGuuid PluginId = new();

    [XmlElement]
    public string RequiredVersionRange = string.Empty;
}

public class PluginManifestWithDependency
{
    [XmlElement]
    public string TypeName = string.Empty;

    [XmlElement]
    public string[] AssemblyLoad = [];

    [XmlElement]
    public XmlGuuid PluginId = new();

    [XmlElement]
    public string PluginVersion { get; set; } = string.Empty;

    [XmlArray("Dependencies")]
    [XmlArrayItem("Dependency")]
    public PluginDependency[] Dependencies { get; set; } = [];
}

/// <summary>
/// 清单文件,负责提供插件的信息
/// </summary>
[XmlRoot]
public class Manifest
{
    [XmlElement]
    public PluginManifestWithDependency[] Plugins { get; set; } = [];
}
