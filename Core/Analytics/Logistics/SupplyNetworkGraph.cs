using System;
using System.Collections.Generic;
using System.Linq;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.Analytics.Logistics;

public enum TransportVesselType
{
    StandardCargo_xAK,
    SmallCargo_xAKL,
    FastTransport_APD,
    FleetOiler_AO_TK,
    AirTransport_Hudson_C47
}

public sealed record SupplyEdge(
    Guid RouteId,
    string SourceBaseName,
    HexCoordinate SourceCoord,
    string DestinationBaseName,
    HexCoordinate DestinationCoord,
    TransportVesselType VesselType,
    int CapacityTons,
    bool IsSevered,
    DateTime LastActiveUtc
);

public sealed record LogisticalNodeStatus(
    string BaseName,
    HexCoordinate Coordinate,
    int InboundCapacityTons,
    int OutboundCapacityTons,
    bool IsStarved,
    bool IsCaptured,
    IReadOnlyList<SupplyEdge> InboundRoutes
);

/// <summary>
/// Directed graph representation of the theater logistical network.
/// </summary>
public sealed class SupplyNetworkGraph
{
    private readonly Dictionary<HexCoordinate, List<SupplyEdge>> _outboundEdges = new();
    private readonly Dictionary<HexCoordinate, List<SupplyEdge>> _inboundEdges = new();
    private readonly Dictionary<HexCoordinate, string> _baseNames = new();
    private readonly HashSet<HexCoordinate> _severedNodes = new();

    public void RegisterBase(string baseName, HexCoordinate coordinate)
    {
        _baseNames[coordinate] = baseName;
    }

    public void AddOrUpdateRoute(
        string srcName, 
        HexCoordinate srcCoord, 
        string dstName, 
        HexCoordinate dstCoord, 
        TransportVesselType type, 
        int capacityTons)
    {
        RegisterBase(srcName, srcCoord);
        RegisterBase(dstName, dstCoord);

        var edge = new SupplyEdge(
            Guid.NewGuid(),
            srcName,
            srcCoord,
            dstName,
            dstCoord,
            type,
            capacityTons,
            _severedNodes.Contains(srcCoord) || _severedNodes.Contains(dstCoord),
            DateTime.UtcNow
        );

        if (!_outboundEdges.ContainsKey(srcCoord)) _outboundEdges[srcCoord] = new();
        if (!_inboundEdges.ContainsKey(dstCoord)) _inboundEdges[dstCoord] = new();

        _outboundEdges[srcCoord].Add(edge);
        _inboundEdges[dstCoord].Add(edge);
    }

    public void MarkNodeSevered(HexCoordinate coordinate)
    {
        _severedNodes.Add(coordinate);
    }

    public IReadOnlyList<LogisticalNodeStatus> EvaluateNetworkHealth()
    {
        var statuses = new List<LogisticalNodeStatus>();

        foreach (var (coord, baseName) in _baseNames)
        {
            bool capturedOrSevered = _severedNodes.Contains(coord);
            var inEdges = _inboundEdges.TryGetValue(coord, out var inList) ? inList : new List<SupplyEdge>();
            var outEdges = _outboundEdges.TryGetValue(coord, out var outList) ? outList : new List<SupplyEdge>();

            int activeInbound = inEdges.Where(e => !e.IsSevered && !_severedNodes.Contains(e.SourceCoord)).Sum(e => e.CapacityTons);
            int activeOutbound = outEdges.Where(e => !e.IsSevered && !_severedNodes.Contains(e.DestinationCoord)).Sum(e => e.CapacityTons);

            bool isStarved = !capturedOrSevered && activeInbound == 0 && inEdges.Count > 0;

            statuses.Add(new LogisticalNodeStatus(
                baseName,
                coord,
                activeInbound,
                activeOutbound,
                isStarved,
                capturedOrSevered,
                inEdges.AsReadOnly()
            ));
        }

        return statuses.AsReadOnly();
    }
}
