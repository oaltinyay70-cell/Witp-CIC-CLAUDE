using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WitpCic.Core.State;

namespace WitpCic.Core.Analytics.Rules;

public enum AlertSeverity
{
    Advisory,
    Warning,
    Critical
}

public sealed record RuleEvaluationResult(
    string RuleId,
    string RuleName,
    bool IsTriggered,
    AlertSeverity Severity,
    string Title,
    string Recommendation,
    IReadOnlyList<string> AffectedEntities
);

/// <summary>
/// Domain rules interface for async world state evaluation and automated recommendation generation.
/// </summary>
public interface IIntelRule
{
    string RuleId { get; }
    string RuleName { get; }
    int Priority { get; }

    ValueTask<RuleEvaluationResult> EvaluateAsync(WorldStateSnapshot state);
}
