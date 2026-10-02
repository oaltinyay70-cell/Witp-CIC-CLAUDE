using System;
using System.Collections.Generic;

namespace WitpCic.Core.Analytics.EntityResolution;

public enum UnitLifecycleState
{
    Active,
    Damaged,
    FloodingPumpsFailed,
    UnderRepair,
    Sinking,
    Sunk,
    Destroyed
}

public enum UnitLifecycleTrigger
{
    ReportDamaged,
    PumpFailure,
    FloodingIncreased,
    BeginRepair,
    CompleteRepair,
    ReportSunk,
    ReportDestroyed
}

public sealed class InvalidStateTransitionException : Exception
{
    public UnitLifecycleState CurrentState { get; }
    public UnitLifecycleTrigger Trigger { get; }

    public InvalidStateTransitionException(UnitLifecycleState current, UnitLifecycleTrigger trigger)
        : base($"Invalid state transition: Cannot execute trigger '{trigger}' from state '{current}'.")
    {
        CurrentState = current;
        Trigger = trigger;
    }
}

/// <summary>
/// Domain state machine enforcing strict lifecycle transitions and cascading damage loss prediction.
/// </summary>
public sealed class EntityStateMachine
{
    private static readonly Dictionary<UnitLifecycleState, Dictionary<UnitLifecycleTrigger, UnitLifecycleState>> Transitions = new()
    {
        [UnitLifecycleState.Active] = new()
        {
            [UnitLifecycleTrigger.ReportDamaged] = UnitLifecycleState.Damaged,
            [UnitLifecycleTrigger.PumpFailure] = UnitLifecycleState.FloodingPumpsFailed,
            [UnitLifecycleTrigger.BeginRepair] = UnitLifecycleState.UnderRepair,
            [UnitLifecycleTrigger.ReportSunk] = UnitLifecycleState.Sunk,
            [UnitLifecycleTrigger.ReportDestroyed] = UnitLifecycleState.Destroyed
        },
        [UnitLifecycleState.Damaged] = new()
        {
            [UnitLifecycleTrigger.PumpFailure] = UnitLifecycleState.FloodingPumpsFailed,
            [UnitLifecycleTrigger.FloodingIncreased] = UnitLifecycleState.FloodingPumpsFailed,
            [UnitLifecycleTrigger.BeginRepair] = UnitLifecycleState.UnderRepair,
            [UnitLifecycleTrigger.ReportSunk] = UnitLifecycleState.Sunk,
            [UnitLifecycleTrigger.ReportDestroyed] = UnitLifecycleState.Destroyed
        },
        [UnitLifecycleState.FloodingPumpsFailed] = new()
        {
            [UnitLifecycleTrigger.FloodingIncreased] = UnitLifecycleState.Sinking,
            [UnitLifecycleTrigger.ReportSunk] = UnitLifecycleState.Sunk,
            [UnitLifecycleTrigger.BeginRepair] = UnitLifecycleState.UnderRepair
        },
        [UnitLifecycleState.UnderRepair] = new()
        {
            [UnitLifecycleTrigger.CompleteRepair] = UnitLifecycleState.Active,
            [UnitLifecycleTrigger.ReportDamaged] = UnitLifecycleState.Damaged,
            [UnitLifecycleTrigger.ReportSunk] = UnitLifecycleState.Sunk
        },
        [UnitLifecycleState.Sinking] = new()
        {
            [UnitLifecycleTrigger.ReportSunk] = UnitLifecycleState.Sunk
        },
        [UnitLifecycleState.Sunk] = new(),
        [UnitLifecycleState.Destroyed] = new()
    };

    public UnitLifecycleState CurrentState { get; private set; }
    public DateTime? InitialDamageTimestamp { get; private set; }
    public DateTime? PumpFailureTimestamp { get; private set; }

    public EntityStateMachine(UnitLifecycleState initialState = UnitLifecycleState.Active)
    {
        CurrentState = initialState;
    }

    public UnitLifecycleState Fire(UnitLifecycleTrigger trigger, DateTime eventTime)
    {
        if (Transitions.TryGetValue(CurrentState, out var allowed) && allowed.TryGetValue(trigger, out var nextState))
        {
            if (trigger == UnitLifecycleTrigger.ReportDamaged && !InitialDamageTimestamp.HasValue)
                InitialDamageTimestamp = eventTime;

            if (trigger == UnitLifecycleTrigger.PumpFailure && !PumpFailureTimestamp.HasValue)
                PumpFailureTimestamp = eventTime;

            CurrentState = nextState;
            return CurrentState;
        }

        throw new InvalidStateTransitionException(CurrentState, trigger);
    }

    /// <summary>
    /// Predicts total loss probability based on damage cascading markers.
    /// </summary>
    public double CalculatePredictedLossProbability()
    {
        return CurrentState switch
        {
            UnitLifecycleState.Sunk or UnitLifecycleState.Destroyed => 1.0,
            UnitLifecycleState.Sinking => 0.95,
            UnitLifecycleState.FloodingPumpsFailed => 0.80,
            UnitLifecycleState.Damaged => 0.35,
            UnitLifecycleState.UnderRepair => 0.10,
            _ => 0.0
        };
    }
}
