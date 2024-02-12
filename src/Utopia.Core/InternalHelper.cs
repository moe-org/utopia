// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Runtime.CompilerServices;
using Utopia.Shared;

[assembly: InternalsVisibleTo("Utopia.Server")]
[assembly: InternalsVisibleTo("Utopia.Server.CorePlugin")]
[assembly: InternalsVisibleTo("Utopia.Godot")]
[assembly: InternalsVisibleTo("Utopia.Godot.CorePlugin")]
[assembly: InternalsVisibleTo("Utopia.Tool")]
[assembly: InternalsVisibleTo("Utopia.Build")]
[assembly: InternalsVisibleTo("Utopia.Analyzer")]

namespace Utopia.Core;
internal static class InternalHelper
{
    internal static Guuid NewInternalGuuid(params string[] nodes)
    {
        return new Guuid(GuuidStandard.UtopiaRoot, nodes);
    }
}
