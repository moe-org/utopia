// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Utopia.Core;

/// <summary>
/// 一组用于XML的工具函数以及一些命名空间信息。
/// </summary>
public static class Xml
{
    /// <summary>
    /// For utopia
    /// </summary>
    public const string Namespace = "http://utopia.kawayi.moe";

    public static XmlSchemas GetXmlSchema<T>()
    {
        return GetXmlSchema(typeof(T));
    }

    public static XmlSchemas GetXmlSchema(Type t)
    {
        XmlSchemas schemas = [];
        XmlSchemaExporter exporter = new(schemas);

        XmlTypeMapping mapping = new XmlReflectionImporter().ImportTypeMapping(t);
        exporter.ExportTypeMapping(mapping);

        return schemas;
    }

    public static void WriteXmlSchemas(XmlSchemas schemas,Stream output)
    {
        foreach (XmlSchema schema in schemas)
        {
            schema.Write(output);
        }
    }

    public static T Deserialize<T>(this XmlSerializer serializer,Stream stream)
    {
        var obj = serializer.Deserialize(stream);

        if(obj is null)
        {
            throw new XmlException("XmlSerializer.Deserialize() returns null");
        }

        return (T)obj;
    }
}