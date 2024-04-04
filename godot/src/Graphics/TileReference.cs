// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Graphics;

namespace Utopia.Godot.Graphics;

public class TileReference(TilePack pack,ulong index)
{
    public TilePack Pack { get; init; } = pack;

    public ulong Index { get; init; } = index;
}
