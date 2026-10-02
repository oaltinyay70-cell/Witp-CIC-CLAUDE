using System;
using System.Collections.Generic;
using System.Linq;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.Analytics.Continuity;

public enum ContinuityConfidence
{
    VerifiedContinuous,
    InterpolatedGap,
    LostContact
}

public sealed record TemporalGapWarningEvent(
    DateOnly StartDate,
    DateOnly EndDate,
    int MissingDaysCount,
    IReadOnlyList<DateOnly> MissingDates
);

public sealed record InterpolatedUnitTrajectory(
    Guid UnitId,
    string UnitName,
    DateOnly GapDate,
    HexCoordinate InterpolatedHex,
    ContinuityConfidence Confidence,
    string Note
);

/// <summary>
/// Scans the timeline of operational logs, detects missing days, and calculates interpolated unit states.
/// </summary>
public sealed class TemporalContinuityChecker
{
    private readonly SortedDictionary<DateOnly, string> _ingestedTurns = new();

    public event EventHandler<TemporalGapWarningEvent>? TemporalGapDetected;

    public void RegisterTurn(DateOnly turnDate, string filePath)
    {
        _ingestedTurns[turnDate] = filePath;
    }

    /// <summary>
    /// Scans for gaps between registered turn dates and fires events for missing days.
    /// </summary>
    public IReadOnlyList<DateOnly> DetectGaps()
    {
        var missingDates = new List<DateOnly>();
        if (_ingestedTurns.Count <= 1) return missingDates;

        var dates = _ingestedTurns.Keys.ToList();
        for (int i = 0; i < dates.Count - 1; i++)
        {
            var current = dates[i];
            var next = dates[i + 1];

            int dayDiff = next.DayNumber - current.DayNumber;
            if (dayDiff > 1)
            {
                var localMissing = new List<DateOnly>();
                for (int d = 1; d < dayDiff; d++)
                {
                    var missing = DateOnly.FromDayNumber(current.DayNumber + d);
                    localMissing.Add(missing);
                    missingDates.Add(missing);
                }

                TemporalGapDetected?.Invoke(this, new TemporalGapWarningEvent(
                    current,
                    next,
                    dayDiff - 1,
                    localMissing.AsReadOnly()
                ));
            }
        }

        return missingDates.AsReadOnly();
    }

    /// <summary>
    /// Linearly interpolates a unit position across missing turn dates.
    /// </summary>
    public InterpolatedUnitTrajectory InterpolatePosition(
        Guid unitId, 
        string unitName, 
        DateOnly beforeDate, 
        HexCoordinate beforeHex, 
        DateOnly afterDate, 
        HexCoordinate afterHex, 
        DateOnly targetGapDate)
    {
        int totalDays = afterDate.DayNumber - beforeDate.DayNumber;
        int elapsedDays = targetGapDate.DayNumber - beforeDate.DayNumber;

        if (totalDays <= 0 || elapsedDays <= 0 || elapsedDays >= totalDays)
        {
            return new InterpolatedUnitTrajectory(
                unitId, 
                unitName, 
                targetGapDate, 
                beforeHex, 
                ContinuityConfidence.LostContact, 
                "Outside valid gap boundaries."
            );
        }

        float fraction = (float)elapsedDays / totalDays;
        int interpX = (int)MathF.Round(beforeHex.X + (afterHex.X - beforeHex.X) * fraction);
        int interpY = (int)MathF.Round(beforeHex.Y + (afterHex.Y - beforeHex.Y) * fraction);

        return new InterpolatedUnitTrajectory(
            unitId,
            unitName,
            targetGapDate,
            new HexCoordinate(interpX, interpY),
            ContinuityConfidence.InterpolatedGap,
            $"Linear interpolation between {beforeDate} and {afterDate} ({fraction:P0} transit)."
        );
    }
}
