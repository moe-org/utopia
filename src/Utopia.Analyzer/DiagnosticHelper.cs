// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Utopia.Shared;

namespace Utopia.Analyzer;

internal static class DiagnosticHelper
{
    private static readonly Regex _regex = new(GuuidStandard.Pattern);

    public static void Debug(this GeneratorExecutionContext context, string fmt, params object[] args)
    {
        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                                                       "UTOPIA00",
                                                       "Analyser DEBUG",
                                                       "Debug message:{0}",
                                                       "Utopia.Analyzer",
                                                       DiagnosticSeverity.Warning,
                                                       true),
                                                   null, string.Format(fmt, args)));
    }

    public static Diagnostic CanNotReadFile(AdditionalText file)
    {
        return Diagnostic.Create(new DiagnosticDescriptor(
                                                               "UTOPIA01",
                                                               "can not read file",
                                                               "AdditionalText.GetText() returns null at file:{0}",
                                                               "Utopia.Analyzer",
                                                               DiagnosticSeverity.Error,
                                                               true),
                                             null, Path.GetFullPath(file.Path));
    }

    public static Diagnostic CanNotDeserialize(AdditionalText file, string msg)
    {
        return Diagnostic.Create(new DiagnosticDescriptor(
                                                               "UTOPIA02",
                                                               "Deserialize failed",
                                                               "Deserialize failed (at file:{0}):{1}",
                                                               "Utopia.Analyzer",
                                                               DiagnosticSeverity.Error,
                                                               true),
                                             null, Path.GetFullPath(file.Path), msg);
    }

    public static void CheckGuuidFormat(string guuid, GeneratorExecutionContext context)
    {
        if (_regex.IsMatch(guuid))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                                                       "UTOPIA03",
                                                       "The guuid format is illegal",
                                                       "The guuid format is illegal:{0}",
                                                       "Utopia.Analyzer",
                                                       DiagnosticSeverity.Error,
                                                       true),
                                                   null, guuid));
    }

    public static Diagnostic TooManyPluginInformationFile()
    {
        return Diagnostic.Create(new DiagnosticDescriptor(
                                     "UTOPIA04",
                                     "Too many plugin information files",
                                     "Too many plugin information files. None will be generated.",
                                     "Utopia.Analyzer",
                                     DiagnosticSeverity.Error,
                                     true),
                                 null);
    }

    public static Diagnostic NoPluginInformationFile()
    {
        return Diagnostic.Create(new DiagnosticDescriptor(
                                     "UTOPIA04",
                                     "No plugin information file found",
                                     "No plugin information file found.",
                                     "Utopia.Analyzer",
                                     DiagnosticSeverity.Error,
                                     true),
                                 null);
    }

}
