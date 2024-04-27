// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Utopia.Core;
using Utopia.Core.Graphics;

namespace Utopia.Godot.Graphics;

/// <summary>
/// A tile set image file include a <see cref="Utopia.Core.Graphics.TileConfiguration"/>.
/// </summary>
public interface ITileSet
{
    TileSetSource Source { get; }

    IEnumerable<Guuid> Tiles { get; }

    bool TryGetTile(Guuid id, [NotNullWhen(true)] out int? index);
}

