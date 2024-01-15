// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Plugin;

namespace Utopia.Core.Exceptions;
public class PluginLoadException : Exception
{
    public IPluginDependencyInformation Plugin { get; init; }

    public PluginLoadException(
        IPluginDependencyInformation information,
        string msg,
        Exception? inner = null) : base(msg, inner)
    {
        Plugin = information;
    }
}
