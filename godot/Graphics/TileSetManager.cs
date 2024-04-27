// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Utopia.Core;

namespace Utopia.Godot.Graphics;

/// <summary>
/// 图集管理器.
/// Not thread safe.
/// </summary>
public class TileSetManager
{
    public TileSet Set { get; init; } = new();

    private Dictionary<int, ITileSet> TileSets { get; init; } = [];

    private Dictionary<Guuid, TileReference> Tiles { get; init; } = [];

    private HashSet<ITileSet> Sets { get; init; } = [];

    public IEnumerable<ITileSet> AllTilePacks => Sets.ToArray();

    public void AddTileSet(ITileSet set)
    {
        if (!Sets.Add(set))
        {
            return;
        }

        var id = Set.AddSource(set.Source);

        TileSets[id] = set;
        foreach (var tile in set.Tiles)
        {
            if (set.TryGetTile(tile, out var index))
            {
                Tiles.Add(tile, new(id, index.Value));
            }
            else
            {
                throw new NotImplementedException("The Tileset declared a tile that is not in the tileset");
            }
        }
    }

    public bool FindTile(Guuid id, [NotNullWhen(true)] out TileReference? reference)
    {
        var result = Tiles.TryGetValue(id, out var @ref);
        reference = @ref;
        return result;
    }
}

