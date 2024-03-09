#region

using System.Reflection;
using Version = SemanticVersioning.Version;

#endregion

namespace Utopia.Core;

public static class VersionUtility
{
    private static readonly Lazy<Version> Version = new(() =>
    {
        // try get from GitVersion first
        var assembly = Assembly.GetCallingAssembly();
        var gitVersionInformationType = assembly.GetType("GitVersionInformation");

        if (gitVersionInformationType is null) goto from_assembly;

        var fields = gitVersionInformationType.GetFields().ToDictionary(field => field.Name);

        if (fields.TryGetValue("SemVer", out var info))
            return SemanticVersioning.Version.Parse((string?)info.GetValue(null) ?? string.Empty);

        from_assembly:

        var v = assembly.GetName().Version ?? new System.Version(0, 1, 0, 0);

        return SemanticVersioning.Version.Parse($"{v.Major}.{v.Minor}.{v.Revision}-{v.Build}");
    }, true);

    public static Version UtopiaCoreVersion => Version.Value;
}
