using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.State;

namespace WitpCic.Core.Analytics.Rules;

/// <summary>
/// Detects concentrated naval forces at choke points and recommends air reconnaissance or strike sorties.
/// </summary>
public sealed class NavalBuildupRule : IIntelRule
{
    public string RuleId => "RULE_NAVAL_BUILDUP_001";
    public string RuleName => "Naval Choke Point Assembly Rule";
    public int Priority => 10;

    public ValueTask<RuleEvaluationResult> EvaluateAsync(WorldStateSnapshot state)
    {
        var surfaceFleets = state.Units.Where(u => u.Type == EntityType.SurfaceShip && u.Coordinate.IsValid).ToList();
        var heavyConcentrations = surfaceFleets
            .GroupBy(u => u.Coordinate)
            .Where(g => g.Count() >= 5)
            .ToList();

        if (heavyConcentrations.Count > 0)
        {
            var prime = heavyConcentrations.First();
            var affected = prime.Select(u => u.Name).ToList();

            return ValueTask.FromResult(new RuleEvaluationResult(
                RuleId,
                RuleName,
                IsTriggered: true,
                Severity: AlertSeverity.Critical,
                Title: $"CRITICAL ALERT: Aggregate naval buildup at ({prime.Key})",
                Recommendation: "Recommend immediate high-altitude air reconnaissance and vectoring strike air groups.",
                AffectedEntities: affected.AsReadOnly()
            ));
        }

        return ValueTask.FromResult(new RuleEvaluationResult(RuleId, RuleName, false, AlertSeverity.Advisory, string.Empty, string.Empty, Array.Empty<string>()));
    }
}

/// <summary>
/// Detects theater-wide logistical fuel exhaustion across task forces.
/// </summary>
public sealed class LogisticsFuelDeficitRule : IIntelRule
{
    public string RuleId => "RULE_LOGISTICS_FUEL_002";
    public string RuleName => "Theater Fuel Exhaustion Rule";
    public int Priority => 20;

    public ValueTask<RuleEvaluationResult> EvaluateAsync(WorldStateSnapshot state)
    {
        var lowFuelTfs = state.Units
            .Where(u => u.Type == EntityType.TaskForce && u.FuelLevel < 20)
            .Select(u => u.Name)
            .ToList();

        if (lowFuelTfs.Count >= 5)
        {
            return ValueTask.FromResult(new RuleEvaluationResult(
                RuleId,
                RuleName,
                IsTriggered: true,
                Severity: AlertSeverity.Warning,
                Title: $"LOGISTICS WARNING: {lowFuelTfs.Count} Task Forces reporting critical fuel exhaustion",
                Recommendation: "Theater-wide fuel deficit detected. Reroute available fleet replenishment oilers (AO/TK) immediately.",
                AffectedEntities: lowFuelTfs.AsReadOnly()
            ));
        }

        return ValueTask.FromResult(new RuleEvaluationResult(RuleId, RuleName, false, AlertSeverity.Advisory, string.Empty, string.Empty, Array.Empty<string>()));
    }
}

/// <summary>
/// Recommends defensive minefield deployment along strategic waterways.
/// </summary>
public sealed class MinefieldReplicationRule : IIntelRule
{
    public string RuleId => "RULE_MINEFIELD_003";
    public string RuleName => "Choke Point Minefield Recommendation";
    public int Priority => 30;

    public ValueTask<RuleEvaluationResult> EvaluateAsync(WorldStateSnapshot state)
    {
        // Example trigger: Base expansion or heavy traffic corridor without mine coverage
        if (state.BaseFortifications.TryGetValue("Singapore", out int fort) && fort >= 1)
        {
            return ValueTask.FromResult(new RuleEvaluationResult(
                RuleId,
                RuleName,
                IsTriggered: true,
                Severity: AlertSeverity.Advisory,
                Title: "STRATEGIC ADVISORY: Strategic Choke Point Minefield Deployment",
                Recommendation: "Task Force 448 laid minefield at Mersing. Replicate offensive minelaying in Malacca Straits and Gaspar Strait.",
                AffectedEntities: new[] { "Singapore", "Mersing", "TF 448" }
            ));
        }

        return ValueTask.FromResult(new RuleEvaluationResult(RuleId, RuleName, false, AlertSeverity.Advisory, string.Empty, string.Empty, Array.Empty<string>()));
    }
}

/// <summary>
/// Detects partisan activity and rear-area garrison vulnerabilities.
/// </summary>
public sealed class PartisanVulnerabilityRule : IIntelRule
{
    public string RuleId => "RULE_PARTISAN_004";
    public string RuleName => "Partisan Threat & Rear Security Rule";
    public int Priority => 40;

    public ValueTask<RuleEvaluationResult> EvaluateAsync(WorldStateSnapshot state)
    {
        var partisanAlerts = state.LogisticalAlerts
            .Where(a => a.Contains("partisan", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (partisanAlerts.Count > 0)
        {
            return ValueTask.FromResult(new RuleEvaluationResult(
                RuleId,
                RuleName,
                IsTriggered: true,
                Severity: AlertSeverity.Warning,
                Title: $"SECURITY WARNING: {partisanAlerts.Count} rear-area cities afflicted by partisan unrest",
                Recommendation: "Allocate rear-area security battalions or base force garrisons to suppress VP loss.",
                AffectedEntities: partisanAlerts.AsReadOnly()
            ));
        }

        return ValueTask.FromResult(new RuleEvaluationResult(RuleId, RuleName, false, AlertSeverity.Advisory, string.Empty, string.Empty, Array.Empty<string>()));
    }
}
