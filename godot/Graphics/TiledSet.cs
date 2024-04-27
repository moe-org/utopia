// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Mingmoe.Tiled;
using Utopia.Core;
using Utopia.Shared;
using TileSet = Mingmoe.Tiled.TileSet;

namespace Utopia.Godot.Graphics;

public class TiledSet : ITileSet
{
    public required TileSetSource Source { get; init; }

    public required IEnumerable<Guuid> Tiles { get; init; }

    public required FrozenDictionary<Guuid, int> TileMap { get; init; }

    public required global::Godot.Image Image { get; init; }

    public static TiledSet LoadTsxFile(string path)
    {
        TileSet tileSet = TileSet.FromFile(path);

        if (tileSet.Image == null)
        {
            throw new TiledException("the image of the tileset is null");
        }

        var image = global::Godot.Image.LoadFromFile(tileSet.Image.Source);
        var texture = ImageTexture.CreateFromImage(image);

        TileSetAtlasSource source = new()
        {
            Texture = texture,
            Separation = new(tileSet.Tilewidth, tileSet.Tileheight)
        };

        Dictionary<Guuid, int> tiles = [];

        foreach (var tile in tileSet.Tiles)
        {
            var guuid = tile.Properties.SingleOrDefault(p => p.Name == Tiled.GuuidPeopertyKey);

            if (guuid is null)
            {
                continue;
            }

            var pos = tileSet.GetPositionOfTile(tile.Id);
            var index = source.CreateAlternativeTile(new(pos.x, pos.y));

            tiles[Guuid.Parse(guuid.Value)] = index;
        }

        return new()
        {
            Image = image,
            TileMap = tiles.ToFrozenDictionary(),
            Source = source,
            Tiles = tiles.Keys
        };
    }

    public bool TryGetTile(Guuid id, [NotNullWhen(true)] out int? index)
    {
        var result = TileMap.TryGetValue(id, out var i);
        index = i;
        return result;
    }
}
