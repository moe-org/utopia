// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core;
using Utopia.Core.Graphics;

namespace Utopia.Godot.Graphics;

/// <summary>
/// A tile pack include a <see cref="Utopia.Core.Graphics.TileConfiguration"/> and its image.
/// </summary>
public class TilePack
{
    public Guuid PackId { get; set; }

    public TileConfiguration Configuration { get; set; }

    public ReadOnlyMemory<byte> Image { get; set; }

    public TilePack(TileConfiguration configuration, ReadOnlyMemory<byte> image)
    {
        Configuration = configuration;
        Image = image;
    }
}

