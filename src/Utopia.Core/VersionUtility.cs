// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Reflection;

namespace Utopia.Core;

public static class VersionUtility
{
    private static Lazy<SemanticVersioning.Version> _version = new(() =>
    {
        // try get from GitVersion first
        var assembly = Assembly.GetCallingAssembly();
        var assemblyName = assembly.GetName().Name;
        var gitVersionInformationType = assembly.GetType("GitVersionInformation");

        if(gitVersionInformationType is null)
        {
            goto from_assembly;
        }

        var fields = gitVersionInformationType.GetFields().ToDictionary((field) => field.Name);

        if(fields.TryGetValue("SemVer", out var info))
        {
            return SemanticVersioning.Version.Parse((string?)info.GetValue(null) ?? string.Empty);
        }

    from_assembly:

        var v = assembly.GetName().Version ?? new Version(0, 1, 0, 0);

        return SemanticVersioning.Version.Parse($"{v.Major}.{v.Minor}.{v.Revision}-{v.Build}");
    }, true);

    public static SemanticVersioning.Version UtopiaCoreVersion { get {
            return _version.Value;
        }
    }
}
