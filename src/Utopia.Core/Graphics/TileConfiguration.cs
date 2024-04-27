// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Graphics;
/// <summary>
/// A configuration for the tile
/// </summary>
public class TileConfiguration
{
    /// <summary>
    /// in pixel
    /// </summary>
    public int Width { get; set; } = 32;

    /// <summary>
    /// in pixel
    /// </summary>
    public int Height { get; set; } = 32;

    public Dictionary<Guuid, TileInformation> Tiles { get; set; } = [];
}
