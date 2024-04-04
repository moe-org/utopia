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
using Utopia.Core;

namespace Utopia.Godot.Graphics;

/// <summary>
/// 贴图管理器
/// </summary>
public class TileManager
{
    public ConcurrentDictionary<string, TilePack> TilePacks { get; init; } = [];

    public bool FindTile(string id, [NotNullWhen(true)] out TileReference? reference)
    {
        foreach(var pack in TilePacks.Values)
        {
            if (pack.Configuration.Tiles.TryGetValue(id,out var value))
            {
                reference = new(pack, value.Index);
                return true;
            }
        }
        reference = null;
        return false;
    }
}

