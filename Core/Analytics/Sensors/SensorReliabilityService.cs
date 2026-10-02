using System;
using System.Collections.Generic;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.Sensors;

/// <summary>
/// Evaluates and assigns dynamic confidence multipliers based on reporting entity and sensor profiles.
/// </summary>
public sealed class SensorReliabilityService
{
    private readonly Dictionary<SensorType, ISensorProfile> _profiles = new();

    public SensorReliabilityService()
    {
        RegisterProfile(new VisualTrainedAirProfile());
        RegisterProfile(new VisualCoastwatcherProfile());
        RegisterProfile(new SurfaceLookoutProfile());
        RegisterProfile(new SigintProfile());
        RegisterProfile(new SelfReportedContactProfile());
        RegisterProfile(new DefaultSensorProfile());
    }

    public void RegisterProfile(ISensorProfile profile)
    {
        _profiles[profile.Type] = profile;
    }

    public SensorType DetectSensorType(string? reporter, string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return SensorType.Unknown;

        ReadOnlySpan<char> textSpan = rawText.AsSpan();
        ReadOnlySpan<char> repSpan = reporter.AsSpan();

        if (textSpan.Contains("reports having been sighted", StringComparison.OrdinalIgnoreCase))
            return SensorType.SelfReportedContact;

        if (repSpan.Contains("SBD", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("PBY", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("Catalina", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("Dauntless", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("Hudson", StringComparison.OrdinalIgnoreCase))
        {
            return SensorType.VisualTrainedAir;
        }

        if (repSpan.Contains("Coastwatcher", StringComparison.OrdinalIgnoreCase) ||
            textSpan.Contains("observes", StringComparison.OrdinalIgnoreCase))
        {
            return SensorType.VisualCoastwatcher;
        }

        if (repSpan.Contains("TF", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("xAK", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("DD", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("CA", StringComparison.OrdinalIgnoreCase) ||
            repSpan.Contains("BB", StringComparison.OrdinalIgnoreCase))
        {
            return SensorType.SurfaceLookout;
        }

        if (textSpan.Contains("Radio transmissions", StringComparison.OrdinalIgnoreCase) ||
            textSpan.Contains("SIGINT", StringComparison.OrdinalIgnoreCase))
        {
            return SensorType.SigintRadioIntercept;
        }

        return SensorType.Unknown;
    }

    public AdjustedConfidenceScore CalculateAdjustedScore(Guid sightingId, string? reporter, string rawText, double rawScore)
    {
        var sensorType = DetectSensorType(reporter, rawText);
        var profile = _profiles.TryGetValue(sensorType, out var p) ? p : _profiles[SensorType.Unknown];

        double adjusted = Math.Clamp(rawScore * profile.ReliabilityMultiplier, 0.0, 1.0);

        return new AdjustedConfidenceScore(
            sightingId,
            sensorType,
            rawScore,
            profile.ReliabilityMultiplier,
            adjusted,
            profile.Description
        );
    }
}
