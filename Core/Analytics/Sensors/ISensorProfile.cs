using System;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.Sensors;

public enum SensorType
{
    VisualTrainedAir,
    VisualCoastwatcher,
    SurfaceLookout,
    SigintRadioIntercept,
    SelfReportedContact,
    Unknown
}

public interface ISensorProfile
{
    SensorType Type { get; }
    double ReliabilityMultiplier { get; }
    string Description { get; }
}

public sealed record AdjustedConfidenceScore(
    Guid SightingId,
    SensorType DetectedSensor,
    double RawConfidence,
    double ReliabilityMultiplier,
    double FinalAdjustedScore,
    string Rationale
);

public sealed class VisualTrainedAirProfile : ISensorProfile
{
    public SensorType Type => SensorType.VisualTrainedAir;
    public double ReliabilityMultiplier => 1.00;
    public string Description => "Trained visual aircrew observation (SBD, PBY, Patrol Recon).";
}

public sealed class VisualCoastwatcherProfile : ISensorProfile
{
    public SensorType Type => SensorType.VisualCoastwatcher;
    public double ReliabilityMultiplier => 0.85;
    public string Description => "Direct land-based coastwatcher / base observation post.";
}

public sealed class SurfaceLookoutProfile : ISensorProfile
{
    public SensorType Type => SensorType.SurfaceLookout;
    public double ReliabilityMultiplier => 0.80;
    public string Description => "Shipboard visual lookout report.";
}

public sealed class SigintProfile : ISensorProfile
{
    public SensorType Type => SensorType.SigintRadioIntercept;
    public double ReliabilityMultiplier => 0.70;
    public string Description => "Cryptographic / radio direction-finding signal intercept.";
}

public sealed class SelfReportedContactProfile : ISensorProfile
{
    public SensorType Type => SensorType.SelfReportedContact;
    public double ReliabilityMultiplier => 0.55;
    public string Description => "Self-reported enemy detection log (hostile sensor unspecified).";
}

public sealed class DefaultSensorProfile : ISensorProfile
{
    public SensorType Type => SensorType.Unknown;
    public double ReliabilityMultiplier => 0.50;
    public string Description => "Unclassified or generic reporting source.";
}
