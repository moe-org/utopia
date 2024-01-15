// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Plugin;
public class UnloadedAssemblyPlugin : IUnloadedPlugin
{
    public required IPluginDependencyInformation Info { get; init; }

    public required IEnumerable<string> Assemblies { get; init; }

    public required string[] TypeName { get; init; }

    public IEnumerable<Type> Load()
    {
        foreach(var assembly in Assemblies)
        {
            Assembly.Load(assembly);
        }

        List<Type> types = new(TypeName.Length);

        foreach(var type in TypeName)
        {
            types.Add(Type.GetType(type,true,true)
                ?? throw new TypeAccessException($"failed to found type {type}"));
        }

        return types;
    }
}
