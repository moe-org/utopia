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
using HarmonyLib;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Zio;

namespace Utopia.Core.Configuration;

public class ConfigurationCompiler
{
    public static readonly IEnumerable<string> DefaultNamespaces =
           [
                "System",
                "System.IO",
                "System.Net",
                "System.Linq",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Collections.Generic",
                "System.Diagnostics"
           ];

    public readonly IList<MetadataReference> References = [];

    private IFileSystem FileSystem { get; init; }

    public ConfigurationCompiler(IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // register current references
        foreach (var assembly in assemblies)
        {
            if (string.IsNullOrEmpty(assembly.Location))
            {
                continue;
            }

            if (!fileSystem.FileExists(assembly.Location))
            {
                continue;
            }

            try
            {
                var file = Assembly.LoadFile(assembly.Location);

                if (file.ImageRuntimeVersion != null)
                {
                    References.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }
            catch (Exception)
            {
                // ignore the dll that we cannot load
            }
        }
    }

    public class GlobalScriptObject<T>(T obj, string? path) where T : class
    {
        public T config { get; init; } = obj;

        public string? scriptPath { get; init; } = path;
    }

    public async Task InvokeFor<T>(string code, T option, IEnumerable<Assembly>? usings = null, IEnumerable<string>? imports = null, string? filePath = null) where T : class
    {
        var scriptOption = ScriptOptions.Default;
        scriptOption = scriptOption.AddReferences(References);

        scriptOption = scriptOption.WithFilePath(filePath ?? "unknown file");

        if (usings != null)
        {
            scriptOption = scriptOption.WithReferences(usings);
        }
        if (imports != null)
        {
            scriptOption = scriptOption.WithImports(imports);
        }

        scriptOption = scriptOption
            .WithImports(DefaultNamespaces);

        var script = CSharpScript.Create(code, scriptOption, typeof(GlobalScriptObject<T>));
        await script.RunAsync(new GlobalScriptObject<T>(option, filePath)).ConfigureAwait(false);

        return;
    }

}
