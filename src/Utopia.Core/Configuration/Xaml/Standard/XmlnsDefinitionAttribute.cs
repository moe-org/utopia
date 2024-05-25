// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Configuration.Xaml.Standard;
using Utopia.Shared;

namespace Utopia.Core.Configuration.Xaml.Standard;

/// <summary>
/// Maps an XML namespace to a CLR namespace for use in XAML.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class XmlnsDefinitionAttribute : Attribute
{
    public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
    {
        XmlNamespace = xmlNamespace;
        ClrNamespace = clrNamespace;
    }

    /// <summary>
    /// Gets or sets the URL of the XML namespace.
    /// </summary>
    public string XmlNamespace { get; }

    /// <summary>
    /// Gets or sets the CLR namespace.
    /// </summary>
    public string ClrNamespace { get; }
}
