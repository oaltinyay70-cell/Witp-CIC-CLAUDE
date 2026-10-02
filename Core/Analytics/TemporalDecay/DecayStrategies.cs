using System;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.TemporalDecay;

/// <summary>
/// Exponential decay strategy based on half-life formula: C(t) = C0 * 2^(-t / t_half)
/// </summary>
public abstract class ExponentialDecayStrategyBase : IDecayStrategy
{
    public abstract TargetClassification HandledClassification { get; }
    public abstract TimeSpan HalfLife { get; }

    public virtual double CalculateConfidence(double initialConfidence, TimeSpan age)
    {
        if (age <= TimeSpan.Zero) return Math.Clamp(initialConfidence, 0.0, 1.0);
        if (HalfLife <= TimeSpan.Zero) return 0.0;

        double halfLivesPassed = age.TotalSeconds / HalfLife.TotalSeconds;
        double decayedScore = initialConfidence * Math.Pow(0.5, halfLivesPassed);

        return Math.Clamp(decayedScore, 0.0, 1.0);
    }
}

/// <summary>
/// Surface fleet contacts decay with a 4-hour half-life due to high naval mobility.
/// </summary>
public sealed class SightingDecayStrategy : ExponentialDecayStrategyBase
{
    public override TargetClassification HandledClassification => TargetClassification.SurfaceShip;
    public override TimeSpan HalfLife => TimeSpan.FromHours(4);
}

/// <summary>
/// Submarine contacts decay with a 2-hour half-life due to submerged evasion.
/// </summary>
public sealed class SubmarineDecayStrategy : ExponentialDecayStrategyBase
{
    public override TargetClassification HandledClassification => TargetClassification.Submarine;
    public override TimeSpan HalfLife => TimeSpan.FromHours(2);
}

/// <summary>
/// Aircraft sightings decay rapidly with a 45-minute half-life.
/// </summary>
public sealed class AircraftDecayStrategy : ExponentialDecayStrategyBase
{
    public override TargetClassification HandledClassification => TargetClassification.Aircraft;
    public override TimeSpan HalfLife => TimeSpan.FromMinutes(45);
}

/// <summary>
/// Minefields are semi-permanent obstacles with a 72-hour half-life.
/// </summary>
public sealed class MinefieldDecayStrategy : ExponentialDecayStrategyBase
{
    public override TargetClassification HandledClassification => TargetClassification.Minefield;
    public override TimeSpan HalfLife => TimeSpan.FromHours(72);
}

/// <summary>
/// Fuel and logistical status reports decay with a 12-hour half-life.
/// </summary>
public sealed class FuelDecayStrategy : ExponentialDecayStrategyBase
{
    public override TargetClassification HandledClassification => TargetClassification.LandUnit;
    public override TimeSpan HalfLife => TimeSpan.FromHours(12);
}

/// <summary>
/// Step-decay strategy for explicit relocation events (e.g. TF leaving patrol zone).
/// </summary>
public sealed class RelocationDecayStrategy : IDecayStrategy
{
    public TargetClassification HandledClassification => TargetClassification.Unknown;
    public TimeSpan HalfLife => TimeSpan.Zero;

    public double CalculateConfidence(double initialConfidence, TimeSpan age)
    {
        return 0.0; // Immediately zeroed
    }
}
