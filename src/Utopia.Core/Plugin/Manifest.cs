#region

using System.Xml.Serialization;

#endregion

namespace Utopia.Core.Plugin;

public class PluginDependency
{
    [XmlElement] public XmlGuuid PluginId = new();

    [XmlElement] public string RequiredVersionRange = string.Empty;
}

public class PluginManifestWithDependency
{
    [XmlElement] public string TypeName = string.Empty;

    [XmlElement] public string[] AssemblyLoad = [];

    [XmlElement] public XmlGuuid PluginId = new();


    [XmlElement] public string PluginVersion { get; set; } = string.Empty;

    [XmlArray("Dependencies")]
    [XmlArrayItem("Dependency")]
    public PluginDependency[] Dependencies { get; set; } = [];
}

/// <summary>
///     清单文件,负责提供插件的信息
/// </summary>
[XmlRoot]
public class Manifest
{
    [XmlElement] public PluginManifestWithDependency[] Plugins { get; set; } = [];
}