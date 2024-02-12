// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;

namespace Utopia.Analyzer;


public static class Utility
{

    public static T? DeserializeXml<T>(AdditionalText file, GeneratorExecutionContext context)
    where T:class
    {
        var text = file.GetText();

        if (text == null)
        {
            context.ReportDiagnostic(DiagnosticHelper.CanNotReadFile(file));
            return null;
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text.ToString()));

        XmlSerializer serializer = new(typeof(T));
        var obj = (T)serializer.Deserialize(stream);

        if (obj is not null)
        {
            return obj;
        }

        context.ReportDiagnostic(
            DiagnosticHelper.CanNotDeserialize(file, "Deserialize() returns null. Ensure the content is right."));
        return null;
    }


}
