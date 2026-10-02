using System;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.TemporalDecay;

/// <summary>
/// Defines mathematical decay curves for intelligence records based on age and event classification.
/// </summary>
public interface IDecayStrategy
{
    TargetClassification HandledClassification { get; }
    TimeSpan HalfLife { get; }

    /// <summary>
    /// Calculates the decayed confidence score [0.0, 1.0] given initial score and elapsed age.
    /// </summary>
    double CalculateConfidence(double initialConfidence, TimeSpan age);
}
