// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Plugin;
public class PluginDependencyInformation : IPluginDependencyInformation
{
    public required Guuid Id { get; init; }

    public required SemanticVersioning.Version Version { get; init; }

    public required (Guuid, SemanticVersioning.Range)[] Dependences { get; init; }
}
