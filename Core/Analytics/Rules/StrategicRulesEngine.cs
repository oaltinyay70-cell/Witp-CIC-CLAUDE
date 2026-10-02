using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WitpCic.Core.State;

namespace WitpCic.Core.Analytics.Rules;

/// <summary>
/// Asynchronous rules evaluation engine that orchestrates decision support rules over World State snapshots.
/// </summary>
public sealed class StrategicRulesEngine
{
    private readonly List<IIntelRule> _rules = new();

    public StrategicRulesEngine()
    {
        // Register default concrete rules
        RegisterRule(new NavalBuildupRule());
        RegisterRule(new LogisticsFuelDeficitRule());
        RegisterRule(new MinefieldReplicationRule());
        RegisterRule(new PartisanVulnerabilityRule());
    }

    public void RegisterRule(IIntelRule rule)
    {
        _rules.Add(rule);
    }

    /// <summary>
    /// Evaluates all registered rules asynchronously in priority order.
    /// </summary>
    public async Task<IReadOnlyList<RuleEvaluationResult>> EvaluateAllAsync(WorldStateSnapshot snapshot)
    {
        var orderedRules = _rules.OrderBy(r => r.Priority).ToList();
        var triggeredAlerts = new List<RuleEvaluationResult>();

        foreach (var rule in orderedRules)
        {
            var result = await rule.EvaluateAsync(snapshot).ConfigureAwait(false);
            if (result.IsTriggered)
            {
                triggeredAlerts.Add(result);
            }
        }

        return triggeredAlerts.AsReadOnly();
    }
}
