// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;
using Coordinate = int;
using WorldId = Utopia.Core.Guuid;

namespace Utopia.Core.Map;

/// <summary>
///     世界位置
/// </summary>
[MemoryPackable]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct WorldPosition
{
    public readonly Coordinate X;

    public readonly Coordinate Y;

    public readonly Coordinate Z;

    /// <summary>
    ///     stand for the World ID
    /// </summary>
    public readonly WorldId Id;

    public WorldPosition(Coordinate x, Coordinate y, Coordinate z, WorldId id)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.Id = id;
    }

    public FlatPosition ToFlat()
    {
        return new FlatPosition(this.X, this.Y);
    }

    public Position ToPos()
    {
        return new Position(this.X, this.Y, this.Z);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is WorldPosition position)
            return position.X == this.X && position.Y == this.Y && position.Z == this.Z && position.Id == this.Id;
        return false;
    }

    public static bool operator ==(WorldPosition lhs, WorldPosition rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(WorldPosition lhs, WorldPosition rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X.GetHashCode(), this.Y.GetHashCode(), this.Z.GetHashCode(),
            this.Id.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})(World Id:{3})", this.X, this.Y, this.Z, this.Id);
    }
}

