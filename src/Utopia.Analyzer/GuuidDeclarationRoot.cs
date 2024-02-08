// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Xml.Serialization;

namespace Utopia.Analyzer;

public class GuuidItem
{
    [XmlElement]
    public string Guuid = "";

    [XmlAttribute]
    public string Description = "No description";

    [XmlAttribute] public string? CSharpName = null;
}

[XmlRoot("GuuidDeclaration")]
public class GuuidDeclarationRoot
{
    [XmlElement]
    public string Namespace = "Utopia";

    [XmlElement]
    public string Class = "GuuidDeclarations";

    [XmlArray("Declarations")]
    [XmlArrayItem("Declaration")]
    public GuuidItem[] Items = [];
}
