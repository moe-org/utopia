// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Utopia.Shared;
using XamlX;
using XamlX.Compiler;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.Transform.Transformers;
using XamlX.TypeSystem;

namespace Utopia.Core.Configuration.Xaml;

public sealed class Compiler : XamlILCompiler
{
    public required Context Context { get; init; }

    public required List<XamlDiagnostic> Diagnostics { get; init; }

    public static Compiler CreateFrom(TransformerConfiguration configuration, List<XamlDiagnostic> diagnostics)
    {
        return new Compiler(configuration, new())
        {
            Context = new((SreTypeSystem)configuration.TypeSystem),
            Diagnostics = diagnostics
        };
    }

    private Compiler(TransformerConfiguration configuration, XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> mappings)
        : base(configuration, mappings, true)
    {
        Transformers.Add(new KnownDirectivesTransformer());
        Transformers.Add(new XamlIntrinsicsTransformer());
        Transformers.Add(new XArgumentsTransformer());
        Transformers.Add(new TypeReferenceResolver());
    }

    private static (Func<IServiceProvider?, object>? create, Action<IServiceProvider?, object?> populate)
        GetCallbacks(Type created)
    {
        var isp = Expression.Parameter(typeof(IServiceProvider));
        var createCb = created.GetMethod("Build") is { } buildMethod
            ? Expression.Lambda<Func<IServiceProvider?, object>>(
                Expression.Convert(Expression.Call(buildMethod, isp), typeof(object)), isp).Compile()
            : null;

        var epar = Expression.Parameter(typeof(object));
        var populate = created.GetMethod("Populate")!;
        isp = Expression.Parameter(typeof(IServiceProvider));
        var populateCb = Expression.Lambda<Action<IServiceProvider?, object?>>(
            Expression.Call(populate, isp, Expression.Convert(epar, populate.GetParameters()[1].ParameterType)),
            isp, epar).Compile();

        return (createCb, populateCb);
    }

    public (Func<IServiceProvider?, object>? create, Action<IServiceProvider?, object?> populate) Compile(string xaml)
    {
        var parsedTypeBuilder = Context.CreateTypeBuilder(Guid.NewGuid().ToString("N"), true);
        var contextTypeBuilder = Context.CreateTypeBuilder(parsedTypeBuilder.XamlTypeBuilder.Name + "Context", false);

        var contextTypeDef = XamlILContextDefinition.GenerateContextClass(
                    contextTypeBuilder.XamlTypeBuilder,
                    _configuration.TypeSystem,
                    _configuration.TypeMappings,
                    new XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>());

        var document = XDocumentXamlParser.Parse(xaml);
        Transform(document);
        Diagnostics.ThrowExceptionIfAnyError();

        Compile(document, parsedTypeBuilder.XamlTypeBuilder, contextTypeDef, "Populate", "Build", "XamlNamespaceInfo", XmlNamespace.Utopia, null);

        parsedTypeBuilder.XamlTypeBuilder.CreateType();
        contextTypeBuilder.XamlTypeBuilder.CreateType();

        return GetCallbacks(parsedTypeBuilder.RuntimeType);
    }

    public T CompileAndCreate<T>(string xaml, IServiceProvider? services = null)
    {
        return (T)Compile(xaml).create!(services);
    }
}
