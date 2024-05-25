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

[MemoryPackable]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct FlatPositionWithId
{
    public readonly Coordinate X;

    public readonly Coordinate Y;

    public readonly WorldId Id;

    public FlatPositionWithId(Coordinate x, Coordinate y, WorldId id)
    {
        this.X = x;
        this.Y = y;
        this.Id = id;
    }

    public FlatPosition ToFlat()
    {
        return new FlatPosition(this.X, this.Y);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is FlatPositionWithId position)
            return position.X == this.X && position.Y == this.Y && position.Id == this.Id;
        return false;
    }

    public static bool operator ==(FlatPositionWithId lhs, FlatPositionWithId rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(FlatPositionWithId lhs, FlatPositionWithId rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X.GetHashCode(), this.Y.GetHashCode(), this.Id.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1})(World Id:{2})", this.X, this.Y, this.Id);
    }
}
