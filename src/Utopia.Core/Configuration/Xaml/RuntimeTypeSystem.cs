// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XamlX.TypeSystem;

namespace Utopia.Core.Configuration.Xaml;
public class RuntimeTypeSystem : IXamlTypeSystem
{
    public RuntimeTypeSystem(Assembly[] assemblies)
    {
        CsAssemblies = assemblies;
    }

    public Assembly[] CsAssemblies { get; init; }

    public IEnumerable<IXamlAssembly> Assemblies => CsAssemblies.Select((a) => new RuntimeAssembly(a));

    public IXamlAssembly FindAssembly(string substring)
    {
        foreach(var assembly in CsAssemblies)
        {
            if (assembly.FullName?.IndexOf(substring, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                return new RuntimeAssembly(assembly);
            }
        }

        return null!;
    }

    public IXamlType FindType([DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] string name)
    {
        foreach (var assembly in CsAssemblies)
        {
            foreach (var type in assembly.ExportedTypes)
            {
                if (type.FullName?.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    return new RuntimeType(new RuntimeAssembly(assembly), type);
                }
            }
        }

        return null!;

    }

    public IXamlType FindType([DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] string name, string assembly)
    {
        var found = (RuntimeAssembly)FindAssembly(assembly);

        if(found is null)
        {
            return null!;
        }

        foreach(var type in found.Assembly.ExportedTypes)
        {
            if(type.FullName?.IndexOf(name,StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                return new RuntimeType(found, type);
            }
        }

        return null!;
    }
}

public class RuntimeAssembly : IXamlAssembly
{
    public RuntimeAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        Assembly = assembly;
    }

    public Assembly Assembly { get; init; }

    public string Name => Assembly.FullName ?? "anonymous-assembly";

    public IReadOnlyList<IXamlCustomAttribute> CustomAttributes => [];

    public bool Equals(IXamlAssembly? other)
    {
        if(other == null || other is not RuntimeAssembly) return false;
        return ((RuntimeAssembly)other).Assembly.Equals(Assembly);
    }

    public IXamlType FindType(string fullName)
    {
        foreach (var type in Assembly.ExportedTypes)
        {
            if (type.FullName!.Equals(fullName, StringComparison.InvariantCultureIgnoreCase))
            {
                return new RuntimeType(this, type);
            }
        }
        return null!;
    }
}

public class RuntimeType : IXamlType
{
    public RuntimeType(IXamlAssembly parent,Type type)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(type);
        Type = type;
        Assembly = parent;
    }

    public Type Type { get; init; }

    public object Id => Type;

    public string Name => Type.Name;

    public string Namespace => Type.Namespace ?? "global";

    public string FullName => Type.FullName ?? "unknown-type";

    public bool IsPublic => Type.IsPublic;

    public bool IsNestedPrivate => Type.IsNestedPrivate;

    public IXamlAssembly Assembly { get; init; }

    public IReadOnlyList<IXamlProperty> Properties => Type.GetProperties().Select((p) => new RuntimeProperty(Assembly,p)).ToList();

    public IReadOnlyList<IXamlEventInfo> Events => [];

    public IReadOnlyList<IXamlField> Fields => [];

    public IReadOnlyList<IXamlMethod> Methods => Type.GetMethods().Select((m)=>new RuntimeMethod(Assembly,m)).ToList(); 

    public IReadOnlyList<IXamlConstructor> Constructors => throw new NotImplementedException();

    public IReadOnlyList<IXamlCustomAttribute> CustomAttributes => [];

    public IReadOnlyList<IXamlType> GenericArguments => [];

    public IXamlType GenericTypeDefinition => this;

    public bool IsArray => Type.IsArray;

    public IXamlType ArrayElementType => (Type.GetElementType() is null ? null : new RuntimeType(Assembly, Type.GetElementType()!))!;

    public IXamlType BaseType => (Type.BaseType is null ? null : new RuntimeType(Assembly, Type.BaseType))!;

    public IXamlType DeclaringType => (Type.DeclaringType is null ? null : new RuntimeType(Assembly, Type.DeclaringType))!;

    public bool IsValueType => Type.IsValueType;

    public bool IsEnum => Type.IsEnum;

    public IReadOnlyList<IXamlType> Interfaces => Type.GetInterfaces().Select((t) => new RuntimeType(Assembly, t)).ToList();

    public bool IsInterface => Type.IsInterface;

    public IReadOnlyList<IXamlType> GenericParameters => Type.GetGenericArguments().Select((t)=>new RuntimeType(Assembly,t)).ToList();

    public bool Equals(IXamlType? other)
    {
        if(other is null || other is not RuntimeType) return false;
        return ((RuntimeType)other).Type.Equals(Type);
    }

    public IXamlType GetEnumUnderlyingType() => new RuntimeType(Assembly, Type.GetEnumUnderlyingType());

    public bool IsAssignableFrom(IXamlType type)
    {
        if(type is RuntimeType t)
        {
            return Type.IsAssignableFrom(t.Type);
        }
        return false;
    }

    public IXamlType MakeArrayType(int dimensions)
    {
        var t = Type;

        while(dimensions != 0)
        {
            t = Type.MakeArrayType();
        }

        return new RuntimeType(Assembly, t);
    }

    public IXamlType MakeGenericType(IReadOnlyList<IXamlType> typeArguments)
    {
        List<Type> ts = new(typeArguments.Count);
        foreach(var t in typeArguments)
        {
            if(t is RuntimeType type)
            {
                ts.Add(type.Type);
            }
            else
            {
                return null!;
            }
        }

        return new RuntimeType(Assembly, Type.MakeGenericType(ts.ToArray()));
    }
}

public class RuntimeMethod : IXamlMethod
{
    public RuntimeMethod(IXamlAssembly assembly,MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(assembly);
        Method = method;
        Assembly = assembly;
    }

    public MethodInfo Method { get; init; }

    public IXamlAssembly Assembly { get; init; }

    public bool IsPublic => Method.IsPublic;

    public bool IsPrivate => Method.IsPrivate;

    public bool IsFamily => Method.IsFamily;

    public bool IsStatic => Method.IsStatic;

    public IXamlType ReturnType => new RuntimeType(Assembly, Method.ReturnType);

    public IReadOnlyList<IXamlType> Parameters => Method.GetParameters().Select((t)=>new RuntimeType(Assembly,t.ParameterType)).ToList();

    public IXamlType DeclaringType => (Method.DeclaringType is null ? null : new RuntimeType(Assembly, Method.DeclaringType))!;

    public IReadOnlyList<IXamlCustomAttribute> CustomAttributes => throw new NotImplementedException();

    public string Name => Method.Name;

    public bool Equals(IXamlMethod? other)
    {
        if(other is null || other is not RuntimeMethod)
        {
            return false;
        }
        return ((RuntimeMethod)other).Method.Equals(Method);
    }
    public IXamlMethod MakeGenericMethod(IReadOnlyList<IXamlType> typeArguments)
    {
        List<Type> ts = new(typeArguments.Count);
        foreach (var t in typeArguments)
        {
            if (t is RuntimeType type)
            {
                ts.Add(type.Type);
            }
            else
            {
                return null!;
            }
        }

        return new RuntimeMethod(Assembly, Method.MakeGenericMethod(ts.ToArray()));

    }
}

public class RuntimeProperty(IXamlAssembly assembly,PropertyInfo propertyInfo) : IXamlProperty
{
    public PropertyInfo Property { get; init; } = propertyInfo;
    public IXamlType PropertyType => new RuntimeType(assembly, Property.PropertyType);

    public IXamlMethod Setter => (Property.GetSetMethod() is null ? null : new RuntimeMethod(assembly, Property.GetSetMethod()!))!;

    public IXamlMethod Getter => (Property.GetGetMethod() is null ? null : new RuntimeMethod(assembly, Property.GetGetMethod()!))!;

    public IReadOnlyList<IXamlCustomAttribute> CustomAttributes => throw new NotImplementedException();

    public IReadOnlyList<IXamlType> IndexerParameters => Property.GetIndexParameters().Select((p) => new RuntimeType(assembly, p.ParameterType)).ToList();

    public string Name => Property.Name;

    public bool Equals(IXamlProperty? other)
    {
        if(other is null || other is not RuntimeProperty)
        {
            return false;
        }

        return ((RuntimeProperty)other).Property.Equals(Property);
    }
}

public class RuntimeConstructor(IXamlAssembly assembly, ConstructorInfo constructor) : IXamlConstructor
{
    public ConstructorInfo Constructor { get; init; } = constructor;

    public bool IsPublic => Constructor.IsPublic;

    public bool IsStatic => Constructor.IsStatic;

    public IReadOnlyList<IXamlType> Parameters => Constructor.GetParameters().Select((p)=>new RuntimeType(assembly,p.ParameterType)).ToList();

    public bool Equals(IXamlConstructor? other)
    {
        if (other is null || other is not RuntimeConstructor) return false;
        return ((RuntimeConstructor)other).Constructor.Equals(Constructor);
    }
}
