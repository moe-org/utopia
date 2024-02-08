#region

using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

#endregion

namespace Utopia.Core.Plugin;

public class PluginDependencyInformation : IPluginDependencyInformation
{
    public required Guuid Id { get; init; }

    public required Version Version { get; init; }

    public required IEnumerable<(Guuid, Range)> Dependencies { get; init; }
}