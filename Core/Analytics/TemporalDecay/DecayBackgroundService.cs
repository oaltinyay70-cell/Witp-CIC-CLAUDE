using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.TemporalDecay;

/// <summary>
/// Thread-safe background temporal decay and purge service using .NET PeriodicTimer.
/// </summary>
public sealed class DecayBackgroundService : IDisposable
{
    private readonly ConcurrentDictionary<Guid, (SightingRecord Record, IDecayStrategy Strategy)> _activeIntel = new();
    private readonly Dictionary<TargetClassification, IDecayStrategy> _strategies = new();
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private Task? _backgroundTask;

    public double DefaultPurgeThreshold { get; set; } = 0.05; // 5% confidence cutoff

    public DecayBackgroundService(TimeSpan tickInterval)
    {
        _timer = new PeriodicTimer(tickInterval);

        // Register default strategies
        RegisterStrategy(new SightingDecayStrategy());
        RegisterStrategy(new SubmarineDecayStrategy());
        RegisterStrategy(new AircraftDecayStrategy());
        RegisterStrategy(new MinefieldDecayStrategy());
        RegisterStrategy(new FuelDecayStrategy());
    }

    public void RegisterStrategy(IDecayStrategy strategy)
    {
        _strategies[strategy.HandledClassification] = strategy;
    }

    public Guid AddIntel(SightingRecord record, IDecayStrategy? customStrategy = null)
    {
        var id = Guid.NewGuid();
        var strategy = customStrategy 
            ?? (_strategies.TryGetValue(record.Classification, out var defaultStrategy) ? defaultStrategy : new SightingDecayStrategy());

        _activeIntel[id] = (record, strategy);
        return id;
    }

    public void Start()
    {
        if (_backgroundTask != null) return;
        _backgroundTask = RunDecayLoopAsync(_cts.Token);
    }

    private async Task RunDecayLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                ApplyDecayAndPurge();
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }

    /// <summary>
    /// Synchronously applies decay math and purges records below threshold.
    /// </summary>
    public void ApplyDecayAndPurge()
    {
        foreach (var kvp in _activeIntel)
        {
            var (record, strategy) = kvp.Value;
            double decayedScore = strategy.CalculateConfidence(record.InitialConfidenceScore, record.Age);

            if (decayedScore <= DefaultPurgeThreshold)
            {
                _activeIntel.TryRemove(kvp.Key, out _);
            }
            else
            {
                var updatedRecord = record with { CurrentConfidenceScore = decayedScore };
                _activeIntel[kvp.Key] = (updatedRecord, strategy);
            }
        }
    }

    /// <summary>
    /// Retrieves active intel filtered by confidence threshold.
    /// </summary>
    public IEnumerable<SightingRecord> GetActiveIntel(double confidenceThreshold = 0.05)
    {
        return _activeIntel.Values
            .Select(v => v.Record)
            .Where(i => i.CurrentConfidenceScore > confidenceThreshold);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Dispose();
        _cts.Dispose();
    }
}
