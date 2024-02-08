// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Plugin;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace Utopia.Tool;

/// <summary>
/// 插件的清单信息
/// </summary>
public class ManifestInformation
{
    public XmlGuuid Id { get; set; }

    public Version Version { get; set; }

    public ManifestInformation(string version,string root,params string[] nodes)
    {
        Id = new XmlGuuid(new Guuid(root,nodes));
        Version = Version.Parse(version);
    }

    public ManifestInformation(System.Version version, string root, params string[] nodes)
    {
        Id = new XmlGuuid(new Guuid(root, nodes));
        Version = new Version(version.Major, version.Minor, version.Revision, null , version.Build.ToString());
    }

    public class RequiredPlugin
    {
        public required XmlGuuid Id { get; set; }

        public required Range RequiredVersion { get; set; }
    }

    public readonly List<RequiredPlugin> Dependencies = [];

    public void AddDependency(string range, string root, string nodes)
    {
        Dependencies.Add(new()
        {
            Id = new XmlGuuid(new Guuid(root,nodes)),
            RequiredVersion = SemanticVersioning.Range.Parse(range)
        });
    }

    public Manifest ToManifest(string[] assemblies,string typeName)
    {
        PluginInfoInManifest information = new()
        {
            Id = Id, Version = Version.ToString(), TypeName = typeName, Assemblies = assemblies
        };
        {
            List<DependencyOfPluginInManifest> deps = [];

            foreach (var dependency in Dependencies)
            {
                deps.Add(new DependencyOfPluginInManifest()
                {
                    Id = dependency.Id, RequiredVersionRange = dependency.RequiredVersion.ToString()
                });
            }

            information.Dependencies = deps.ToArray();
        }

        return new Manifest { Plugins = [information] };
    }
}
