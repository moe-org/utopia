// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XamlX;
using XamlX.IL;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace Utopia.Core.Configuration.Xaml.Standard;

public class StandardConfiguration
{
    private static bool s_loaded = false;

    public static XamlLanguageTypeMappings GetLanguageTypeMappings(IXamlTypeSystem typeSystem)
    {
        return new XamlLanguageTypeMappings(typeSystem)
        {
            XmlnsAttributes =
                [
                    typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.XmlnsDefinitionAttribute"),
                ],
            ContentAttributes =
                [
                    typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.ContentAttribute")
                ],
            TrimSurroundingWhitespaceAttributes =
                [
                    typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.TrimSurroundingWhitespaceAttribute")
                ],
            RootObjectProvider = typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.IRootObjectProvider"),
            UriContextProvider = typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.IUriContext"),
            ProvideValueTarget = typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.IProvideValueTarget"),
            ParentStackProvider = typeSystem.GetType("XamlX.Runtime.IXamlParentStackProviderV1"),
            XmlNamespaceInfoProvider = typeSystem.GetType("XamlX.Runtime.IXamlXmlNamespaceInfoProviderV1"),
            IAddChild = typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.IAddChild"),
            IAddChildOfT = typeSystem.GetType("Utopia.Core.Configuration.Xaml.Standard.IAddChild`1")
        };
    }

    public static (TransformerConfiguration, List<XamlDiagnostic>) CreateDefault()
    {
        if (!s_loaded)
        {
            Assembly.Load("XamlX.Runtime");
        }
        s_loaded = true;

        // we do not care about the thread safe
        // for load the assembly multiple times can work well too

        var typeSystem = new SreTypeSystem();
        List<XamlDiagnostic> diagnostics = [];

        return new(new(typeSystem,
            typeSystem.FindAssembly("Utopia.Core.Configuration.Xaml.Standard"),
            GetLanguageTypeMappings(typeSystem),
            diagnosticsHandler: new XamlDiagnosticsHandler()
            {
                HandleDiagnostic = (d) =>
                {
                    diagnostics.Add(d);
                    return d.Severity;
                }
            }
        ), diagnostics);
    }

}
