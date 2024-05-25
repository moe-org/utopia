#region

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MemoryPack;
using Coordinate = int;
using WorldId = Utopia.Core.Guuid;

#endregion

namespace Utopia.Core.Map;

/// <summary>
/// 三维位置
/// </summary>
[MemoryPackable]
[StructLayout(LayoutKind.Sequential)]
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
