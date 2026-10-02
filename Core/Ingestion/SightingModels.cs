using System;
using System.Collections.Generic;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.Ingestion;

public enum TargetClassification
{
    Unknown,
    SurfaceShip,
    Submarine,
    Aircraft,
    LandUnit,
    Minefield
}

public sealed record SightingRecord(
    DateOnly TurnDate,
    DateTime TimestampUtc,
    HexCoordinate Coordinate,
    string? LocationName,
    TargetClassification Classification,
    string TargetDescription,
    int? ShipCount,
    int? Speed,
    string? Heading,
    string? Reporter,
    int CorroborationCount,
    string PrimaryRawText,
    IReadOnlyList<string> CorroboratingRawLines,
    double InitialConfidenceScore = 1.0
)
{
    public TimeSpan Age => DateTime.UtcNow - TimestampUtc;
    public double CurrentConfidenceScore { get; init; } = InitialConfidenceScore;
}
