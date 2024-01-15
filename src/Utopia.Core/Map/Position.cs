// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Diagnostics.CodeAnalysis;
using MessagePack;

using Coordinate = int;
using WorldId = Utopia.Core.Guuid;

namespace Utopia.Core.Map;

/// <summary>
/// 平面位置
/// </summary>
[MessagePackObject]
public readonly struct FlatPosition
{
    [Key(0)]
    public readonly Coordinate X;
    [Key(1)]
    public readonly Coordinate Y;

    public FlatPosition(Coordinate x, Coordinate y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is FlatPosition position)
        {
            return position.X == X && position.Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1})",X,Y);
    }
}

/// <summary>
/// 三维位置
/// </summary>
[MessagePackObject]
public readonly struct Position
{
    [Key(0)]
    public readonly Coordinate X;
    [Key(1)]
    public readonly Coordinate Y;
    [Key(2)]
    public readonly Coordinate Z;

    public Position(Coordinate x, Coordinate y, Coordinate z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public FlatPosition ToFlat() => new(X, Y);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Position position)
        {
            return position.X == X && position.Y == Y && position.Z == Z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(),Z.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})", X, Y, Z);
    }
}

/// <summary>
/// 世界位置
/// </summary>
[MessagePackObject]
public readonly struct WorldPosition
{
    [Key(0)]
    public readonly Coordinate X;
    [Key(1)]
    public readonly Coordinate Y;
    [Key(2)]
    public readonly Coordinate Z;
    /// <summary>
    /// stand for the World ID
    /// </summary>
    [Key(3)]
    public readonly WorldId Id;

    public WorldPosition(Coordinate x, Coordinate y, Coordinate z, WorldId id)
    {
        X = x;
        Y = y;
        Z = z;
        Id = id;
    }

    public FlatPosition ToFlat() => new(X, Y);

    public Position ToPos() => new(X, Y, Z);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is WorldPosition position)
        {
            return position.X == X && position.Y == Y && position.Z == Z && position.Id == Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(), Z.GetHashCode(),Id.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})(World Id:{3})", X, Y, Z, Id);
    }
}

[MessagePackObject]
public readonly struct FlatPositionWithId
{
    [Key(0)]
    public readonly Coordinate X;
    [Key(1)]
    public readonly Coordinate Y;
    [Key(2)]
    public readonly WorldId Id;

    public FlatPositionWithId(Coordinate x, Coordinate y, WorldId id)
    {
        X = x;
        Y = y;
        Id = id;
    }

    public FlatPosition ToFlat() => new(X, Y);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is FlatPositionWithId position)
        {
            return position.X == X && position.Y == Y && position.Id == Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(), Id.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1})(World Id:{2})", X, Y, Id);
    }
}
