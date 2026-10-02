using System;
using System.Collections.Generic;
using System.Numerics;

namespace WitpCic.Core.Geospatial;

/// <summary>
/// Immutable, zero-allocation hex coordinate representation conforming to WitP axial topology.
/// </summary>
public readonly struct HexCoordinate : IEquatable<HexCoordinate>, IComparable<HexCoordinate>
{
    public int X { get; }
    public int Y { get; }

    public static readonly HexCoordinate Empty = new(0, 0);

    public HexCoordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool IsValid => X > 0 && Y > 0;

    public bool Equals(HexCoordinate other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is HexCoordinate other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public int CompareTo(HexCoordinate other)
    {
        int xComparison = X.CompareTo(other.X);
        return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
    }

    public static bool operator ==(HexCoordinate left, HexCoordinate right) => left.Equals(right);
    public static bool operator !=(HexCoordinate left, HexCoordinate right) => !left.Equals(right);

    public override string ToString() => $"{X},{Y}";

    public Vector2 ToVector2() => new((float)X, (float)Y);

    public static HexCoordinate FromVector2(Vector2 vector) =>
        new((int)MathF.Round(vector.X), (int)MathF.Round(vector.Y));
}
