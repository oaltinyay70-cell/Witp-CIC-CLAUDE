using System;
using System.Collections.Generic;
using System.Linq;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.Bayesian;

public sealed record CorroboratedGroundTruth(
    string ContactKey,
    double CombinedProbability,
    int IndependentSourceCount,
    int CorrelatedSourceCount,
    IReadOnlyList<string> DistinctEntities,
    IReadOnlyList<SightingRecord> Sightings
);

/// <summary>
/// Aggregates multi-observer sighting confidence via Bayesian Noisy-OR fusion with entity correlation gating.
/// </summary>
public sealed class BayesianCorroborationEngine
{
    private readonly EntityResolutionService _entityResolver;
    private const double MaxConfidenceCap = 0.99;

    public BayesianCorroborationEngine(EntityResolutionService entityResolver)
    {
        _entityResolver = entityResolver;
    }

    /// <summary>
    /// Combines multiple sighting probabilities for a single contact target using Noisy-OR formula.
    /// </summary>
    public CorroboratedGroundTruth CombineProbabilities(string contactKey, IEnumerable<SightingRecord> sightings)
    {
        var list = sightings.ToList();
        if (list.Count == 0)
        {
            return new CorroboratedGroundTruth(contactKey, 0.0, 0, 0, Array.Empty<string>(), Array.Empty<SightingRecord>());
        }

        var seenEntityIds = new HashSet<Guid>();
        var distinctEntityNames = new List<string>();
        int independentCount = 0;
        int correlatedCount = 0;

        double combinedFailureProb = 1.0;

        foreach (var s in list)
        {
            string reporter = string.IsNullOrEmpty(s.Reporter) ? "Unknown" : s.Reporter;
            Guid entityId = _entityResolver.ResolveOrCreate(reporter);

            double p = Math.Clamp(s.CurrentConfidenceScore, 0.01, 0.95);

            if (seenEntityIds.Add(entityId))
            {
                // Distinct / Independent observer
                distinctEntityNames.Add(reporter);
                independentCount++;
                combinedFailureProb *= (1.0 - p);
            }
            else
            {
                // Correlated observer (same Task Force or unit) -> apply diminishing returns
                correlatedCount++;
                double correlatedP = p * 0.35;
                combinedFailureProb *= (1.0 - correlatedP);
            }
        }

        double combined = 1.0 - combinedFailureProb;
        double finalScore = Math.Min(combined, MaxConfidenceCap);

        return new CorroboratedGroundTruth(
            contactKey,
            finalScore,
            independentCount,
            correlatedCount,
            distinctEntityNames.AsReadOnly(),
            list.AsReadOnly()
        );
    }
}
