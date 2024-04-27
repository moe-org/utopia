// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlX.IL;
using XamlX.TypeSystem;

namespace Utopia.Core.Configuration.Xaml;
public class RuntimeTypeBuilder
{
    private readonly Func<Type> _createRuntimeType;
    private Type? _runtimeType;

    public RuntimeTypeBuilder(IXamlTypeBuilder<IXamlILEmitter> xamlTypeBuilder, Func<Type> createRuntimeType)
    {
        XamlTypeBuilder = xamlTypeBuilder;
        _createRuntimeType = createRuntimeType;
    }

    public IXamlTypeBuilder<IXamlILEmitter> XamlTypeBuilder { get; }

    public Type RuntimeType
        => _runtimeType ??= _createRuntimeType();

}
