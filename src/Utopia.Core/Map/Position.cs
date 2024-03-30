#region

using System.Diagnostics.CodeAnalysis;
using MemoryPack;
using Coordinate = int;
using WorldId = Utopia.Core.Guuid;

#endregion

namespace Utopia.Core.Map;

/// <summary>
///     平面位置
/// </summary>
[MemoryPackable]
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

/// <summary>
/// 三维位置
/// </summary>
[MemoryPackable]
public readonly partial struct Position
{
    public readonly Coordinate X;

    public readonly Coordinate Y;

    public readonly Coordinate Z;

    public Position(Coordinate x, Coordinate y, Coordinate z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public FlatPosition ToFlat()
    {
        return new FlatPosition(this.X, this.Y);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Position position) return position.X == this.X && position.Y == this.Y && position.Z == this.Z;
        return false;
    }

    public static bool operator ==(Position lhs, Position rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Position lhs, Position rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X.GetHashCode(), this.Y.GetHashCode(), this.Z.GetHashCode());
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})", this.X, this.Y, this.Z);
    }
}

/// <summary>
///     世界位置
/// </summary>
[MemoryPackable]
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

[MemoryPackable]
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
