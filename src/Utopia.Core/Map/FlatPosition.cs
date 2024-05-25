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

namespace Utopia.Core.Map;

using Coordinate = int;

/// <summary>
///     平面位置
/// </summary>
[MemoryPackable]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct FlatPosition
{
    public readonly Coordinate X;

    public readonly Coordinate Y;

    public FlatPosition(Coordinate x, Coordinate y)
    {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is FlatPosition position) return position.X == this.X && position.Y == this.Y;
        return false;
    }

    public static bool operator ==(FlatPosition lhs, FlatPosition rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(FlatPosition lhs, FlatPosition rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X.GetHashCode(), this.Y.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", this.X, this.Y);
    }
}
