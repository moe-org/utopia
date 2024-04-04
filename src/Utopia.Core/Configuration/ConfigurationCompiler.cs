// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Net;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Security.Cryptography.Xml;
using Microsoft.CodeAnalysis.Emit;
using Utopia.Core.Security;

namespace Utopia.Core.Configuration;
public class ConfigurationCompiler
{
    private const string SourceCodeTemplate =
        """
        {header}

        namespace Mingmoe.Configuration.Generated;

        internal sealed class {class name}{

            public static void Configurate(in {config type} config){
                {source code}
            }

            private {class name}(){}
        }
        
        """;

    public Dictionary<string, string> CscOptions => [];

    public List<MetadataReference> References => [];

    public ConfigurationCompiler()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // register current references
        foreach(var assembly in assemblies)
        {
            if(assembly.Location != string.Empty)
            {
                References.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }
    }

    public bool TryCompile<T>(string header,string code, out Diagnostic[] errors,T option)
    {
        var className = $"GeneratedConfigurationClass_{SecureUtilities.GenerateRandomString(16)}";

        code = SourceCodeTemplate
            .Replace("{source code}", code)
            .Replace("{class name}", className)
            .Replace("{header}",header);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

        CSharpCompilation compilation = CSharpCompilation.Create(
                "generated_configuration",
                syntaxTrees: new[] { syntaxTree },
                references: References,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var ms = new MemoryStream())
        {
            // write IL code into memory
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                // handle exceptions
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.Severity == DiagnosticSeverity.Warning || 
                    diagnostic.Severity == DiagnosticSeverity.Error);

                errors = failures.ToArray();
                return false;
            }
            else
            {
                // load this 'virtual' DLL so that we can use
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // create instance of the desired class and call the desired function
                Type type = assembly.GetType($"Mingmoe.Configuration.Generated.{className}")!;
                var method = type.GetMethod("Configurate", BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;

                // invoke
                method.Invoke(null, [option]);
            }
        }

        errors = [];
        return true;
    }

}
