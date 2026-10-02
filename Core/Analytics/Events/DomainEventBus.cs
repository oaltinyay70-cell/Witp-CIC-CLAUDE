using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WitpCic.Core.Analytics.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAtUtc { get; }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent);
}

/// <summary>
/// In-process, zero-network domain event bus implementing the Mediator pattern.
/// </summary>
public sealed class DomainEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<IDomainEvent, Task>>> _handlers = new();

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        var list = _handlers.GetOrAdd(eventType, _ => new List<Func<IDomainEvent, Task>>());
        lock (list)
        {
            list.Add(e => handler((TEvent)e));
        }
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var list))
        {
            List<Func<IDomainEvent, Task>> snapshot;
            lock (list)
            {
                snapshot = new List<Func<IDomainEvent, Task>>(list);
            }

            foreach (var handler in snapshot)
            {
                await handler(domainEvent).ConfigureAwait(false);
            }
        }
    }
}

// Common Domain Events
public sealed record PumpFailureEvent(Guid EventId, DateTime OccurredAtUtc, Guid EntityId, string EntityName) : IDomainEvent;
public sealed record ShipSunkEvent(Guid EventId, DateTime OccurredAtUtc, Guid EntityId, string EntityName) : IDomainEvent;
public sealed record BaseCapturedEvent(Guid EventId, DateTime OccurredAtUtc, string BaseName, string CaptorFaction) : IDomainEvent;
public sealed record LowFuelWarningEvent(Guid EventId, DateTime OccurredAtUtc, string TaskForceName) : IDomainEvent;
