using System;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.State.Hazards;

public enum HazardType
{
    MinefieldDefensive,
    MinefieldOffensive,
    OilSlickAsw,
    WreckageChokePoint
}

public enum Faction
{
    Allied,
    Japanese,
    Neutral
}

public sealed record HazardState(
    Guid HazardId,
    HexCoordinate Coordinate,
    string LocationName,
    HazardType Type,
    Faction OwnerFaction,
    int DensityPercent,
    string LaidByUnit,
    DateTime LaidAtUtc
)
{
    public TimeSpan Age => DateTime.UtcNow - LaidAtUtc;
}
