#region

using System.Reflection;
using Version = SemanticVersioning.Version;

#endregion

namespace Utopia.Core;

public static class VersionUtility
{
    private static readonly Lazy<Version> Version = new(() =>
    {
        var assembly = Assembly.GetCallingAssembly();

        goto from_git_information;

    // try get from GitVersion first
    from_git_information:
        var gitVersionInformationType = assembly.GetType("GitVersionInformation");

        if (gitVersionInformationType is null) goto workaround;

        var fields = gitVersionInformationType.GetFields().ToDictionary(field => field.Name);

        if (fields.TryGetValue("SemVer", out var info))
            return SemanticVersioning.Version.Parse((string?)info.GetValue(null) ?? string.Empty);

        goto workaround;
    // from AssemblyInformationalVersionAttribute
    workaround:
        var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return SemanticVersioning.Version.Parse(attr?.InformationalVersion ?? "0.0.1-unknown.version");
    }, true);

    public static Version UtopiaCoreVersion => Version.Value;
}
