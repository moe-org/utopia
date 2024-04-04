// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using XamlX.Compiler;
using XamlX.Emit;
using XamlX.Transform;
using XamlX.Transform.Transformers;
using XamlX.TypeSystem;

namespace Utopia.Core.Configuration.Xaml;

public sealed class Compiler : XamlCompiler<object, IXamlEmitResult>
{
    public static Compiler CreateDefault(RuntimeTypeSystem typeSystem, params Type[] additionalTypes)
    {
        var mappings = new XamlLanguageTypeMappings(typeSystem);
        foreach (var additionalType in additionalTypes)
        {
            if (!typeSystem.CsAssemblies.Contains(additionalType.Assembly))
            {
                typeSystem = new RuntimeTypeSystem([.. typeSystem.CsAssemblies, additionalType.Assembly]);
            }
            mappings.XmlnsAttributes.Add(new RuntimeType(new RuntimeAssembly(additionalType.Assembly),additionalType));
        }

        var diagnosticsHandler = new XamlDiagnosticsHandler();

        var configuration = new TransformerConfiguration(
            typeSystem,
            typeSystem.Assemblies.First(),
            mappings,
            diagnosticsHandler: diagnosticsHandler);
        return new Compiler(configuration);
    }

    private Compiler(TransformerConfiguration configuration)
        : base(configuration, new XamlLanguageEmitMappings<object, IXamlEmitResult>(), false)
    {
        Transformers.Add(new KnownDirectivesTransformer());
        Transformers.Add(new XamlIntrinsicsTransformer());
        Transformers.Add(new XArgumentsTransformer());
        Transformers.Add(new TypeReferenceResolver());
    }

    protected override XamlEmitContext<object, IXamlEmitResult> InitCodeGen(
        IFileSource file,
        IXamlTypeBuilder<object> declaringType,
        object codeGen,
        XamlRuntimeContext<object, IXamlEmitResult> context,
        bool needContextLocal) =>
        throw new NotSupportedException();
}

