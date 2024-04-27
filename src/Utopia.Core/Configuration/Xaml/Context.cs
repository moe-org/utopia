// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XamlX.IL;
using XamlX.TypeSystem;

namespace Utopia.Core.Configuration.Xaml;

public class Context
{
    public AssemblyBuilder RuntimeAssembly { get; init; }

    public ModuleBuilder Module { get; init; }

    public SreTypeSystem TypeSystem { get; init; }

    public Context(SreTypeSystem typeSystem)
    {
        RuntimeAssembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("UtopiaXamlAssembly." + Guid.NewGuid().ToString("N")),
                AssemblyBuilderAccess.Run);

        TypeSystem = typeSystem;

        Module = RuntimeAssembly.DefineDynamicModule("UtopiaXamlModule." + Guid.NewGuid().ToString("N") + ".dll");
    }

    public RuntimeTypeBuilder CreateTypeBuilder(string name, bool isPublic)
    {
        var attributes = TypeAttributes.Class | (isPublic ? TypeAttributes.Public : TypeAttributes.NotPublic);
        var type = Module.DefineType(name, attributes);
        var typeBuilder = (TypeSystem).CreateTypeBuilder(type);
        return new RuntimeTypeBuilder(typeBuilder, () => type.CreateType()!);
    }
}
