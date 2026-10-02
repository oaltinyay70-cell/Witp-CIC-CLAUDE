using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.State;

/// <summary>
/// Reactive entity state representation supporting WPF/Blazor UI data binding via INotifyPropertyChanged.
/// </summary>
public sealed class UnitState : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private EntityType _type;
    private UnitLifecycleState _lifecycleState;
    private HexCoordinate _coordinate;
    private int _fuelLevel;
    private double _lossProbability;
    private DateTime _lastUpdatedUtc;

    public Guid Id { get; init; }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public EntityType Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    public UnitLifecycleState LifecycleState
    {
        get => _lifecycleState;
        set => SetField(ref _lifecycleState, value);
    }

    public HexCoordinate Coordinate
    {
        get => _coordinate;
        set => SetField(ref _coordinate, value);
    }

    public int FuelLevel
    {
        get => _fuelLevel;
        set => SetField(ref _fuelLevel, value);
    }

    public double LossProbability
    {
        get => _lossProbability;
        set => SetField(ref _lossProbability, value);
    }

    public DateTime LastUpdatedUtc
    {
        get => _lastUpdatedUtc;
        set => SetField(ref _lastUpdatedUtc, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
