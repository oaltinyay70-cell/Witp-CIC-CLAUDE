using System;
using System.Collections.Generic;
using System.Numerics;

namespace WitpCic.Core.Geospatial;

/// <summary>
/// High-performance geospatial math and vector extrapolation extensions for hex grids.
/// </summary>
public static class HexGeoMathExtensions
{
    private const float DefaultNauticalMilesPerHex = 40.0f;

    /// <summary>
    /// Calculates hex distance using axial/Manhattan distance formula for WitP grid topology.
    /// </summary>
    public static int GetDistanceTo(this HexCoordinate origin, HexCoordinate destination)
    {
        if (!origin.IsValid || !destination.IsValid) return int.MaxValue;

        int dx = destination.X - origin.X;
        int dy = destination.Y - origin.Y;

        // Axial hex distance formula
        return (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dx - dy)) / 2;
    }

    /// <summary>
    /// Returns the 6 adjacent neighboring hexes surrounding the specified coordinate.
    /// </summary>
    public static IEnumerable<HexCoordinate> GetAdjacentHexes(this HexCoordinate center)
    {
        if (!center.IsValid) yield break;

        // WitP hex grid standard offset neighbors
        yield return new HexCoordinate(center.X + 1, center.Y);
        yield return new HexCoordinate(center.X - 1, center.Y);
        yield return new HexCoordinate(center.X, center.Y + 1);
        yield return new HexCoordinate(center.X, center.Y - 1);
        yield return new HexCoordinate(center.X + 1, center.Y - 1);
        yield return new HexCoordinate(center.X - 1, center.Y + 1);
    }

    /// <summary>
    /// Converts a compass direction (e.g. "West", "SE", "North") into a normalized Vector2.
    /// </summary>
    public static Vector2 ToBearingVector(this string? heading)
    {
        if (string.IsNullOrWhiteSpace(heading)) return Vector2.Zero;

        ReadOnlySpan<char> span = heading.AsSpan().Trim();
        if (span.Equals("North", StringComparison.OrdinalIgnoreCase) || span.Equals("N", StringComparison.OrdinalIgnoreCase))
            return new Vector2(0, -1);
        if (span.Equals("South", StringComparison.OrdinalIgnoreCase) || span.Equals("S", StringComparison.OrdinalIgnoreCase))
            return new Vector2(0, 1);
        if (span.Equals("East", StringComparison.OrdinalIgnoreCase) || span.Equals("E", StringComparison.OrdinalIgnoreCase))
            return new Vector2(1, 0);
        if (span.Equals("West", StringComparison.OrdinalIgnoreCase) || span.Equals("W", StringComparison.OrdinalIgnoreCase))
            return new Vector2(-1, 0);
        if (span.Equals("NE", StringComparison.OrdinalIgnoreCase) || span.Equals("NorthEast", StringComparison.OrdinalIgnoreCase))
            return Vector2.Normalize(new Vector2(1, -1));
        if (span.Equals("NW", StringComparison.OrdinalIgnoreCase) || span.Equals("NorthWest", StringComparison.OrdinalIgnoreCase))
            return Vector2.Normalize(new Vector2(-1, -1));
        if (span.Equals("SE", StringComparison.OrdinalIgnoreCase) || span.Equals("SouthEast", StringComparison.OrdinalIgnoreCase))
            return Vector2.Normalize(new Vector2(1, 1));
        if (span.Equals("SW", StringComparison.OrdinalIgnoreCase) || span.Equals("SouthWest", StringComparison.OrdinalIgnoreCase))
            return Vector2.Normalize(new Vector2(-1, 1));

        return Vector2.Zero;
    }

    /// <summary>
    /// Extrapolates future hex position given current coordinate, speed in knots, heading, and elapsed hours.
    /// </summary>
    public static HexCoordinate ExtrapolatePosition(
        this HexCoordinate current,
        int speedKnots,
        string? heading,
        float elapsedHours,
        float nauticalMilesPerHex = DefaultNauticalMilesPerHex)
    {
        if (!current.IsValid || speedKnots <= 0 || elapsedHours <= 0) return current;

        Vector2 direction = heading.ToBearingVector();
        if (direction == Vector2.Zero) return current;

        float distanceNauticalMiles = speedKnots * elapsedHours;
        float hexesTraveled = distanceNauticalMiles / nauticalMilesPerHex;

        Vector2 displacement = direction * hexesTraveled;
        Vector2 futureVector = current.ToVector2() + displacement;

        return HexCoordinate.FromVector2(futureVector);
    }
}
