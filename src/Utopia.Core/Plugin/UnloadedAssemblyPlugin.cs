#region

using System.Reflection;

#endregion

namespace Utopia.Core.Plugin;

public class UnloadedAssemblyPlugin : IUnloadedPlugin
{
    public required IEnumerable<string> Assemblies { get; init; }

    public required string[] TypeName { get; init; }
    public required IPluginDependencyInformation Info { get; init; }

    public IEnumerable<Type> Load()
    {
        foreach (var assembly in this.Assemblies) Assembly.Load(assembly);

        List<Type> types = new(this.TypeName.Length);

        foreach (var type in this.TypeName)
            types.Add(Type.GetType(type, true, true)
                      ?? throw new TypeAccessException($"failed to found type {type}"));

        return types;
    }
}
