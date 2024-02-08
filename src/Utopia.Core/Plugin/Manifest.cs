#region

using System.Xml.Serialization;

#endregion

namespace Utopia.Core.Plugin;

public class DependencyOfPluginInManifest
{
    [XmlElement] public XmlGuuid Id = new();

    [XmlElement] public string RequiredVersionRange = string.Empty;
}

public class PluginInfoInManifest
{
    [XmlElement] public string TypeName = string.Empty;

    [XmlElement] public string[] Assemblies = [];

    [XmlElement] public XmlGuuid Id = new();

    [XmlElement] public string Version { get; set; } = string.Empty;

    [XmlArray("Dependencies")]
    [XmlArrayItem("Dependency")]
    public DependencyOfPluginInManifest[] Dependencies { get; set; } = [];
}

/// <summary>
///     清单文件,负责提供插件的信息
/// </summary>
[XmlRoot]
public class Manifest
{
    [XmlElement] public PluginInfoInManifest[] Plugins { get; set; } = [];
}
