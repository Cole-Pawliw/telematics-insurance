using Telematics.Domain.Enums;
using Telematics.Domain.Errors;
using Telematics.Domain.ValueObjects;

namespace Telematics.Domain.Entities;

/// <summary>
/// A completed trip reported by a vehicle.
/// </summary>
public sealed class TripLog
{
    public const int PolicyNumberMaxLength = 32;

    /// <summary>Trips faster than this are flagged for a safety review.</summary>
    public const decimal ReviewSpeedThresholdMph = 100m;

    // Used by EF Core when loading from the database.
    private TripLog()
    {
    }

    public Guid Id { get; private set; }

    public string PolicyNumber { get; private set; } = string.Empty;

    public TripDuration Duration { get; private set; } = null!;

    public DrivingMetrics Metrics { get; private set; } = null!;

    public TripStatus Status { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    public static TripLog Record(
        string policyNumber,
        TripDuration duration,
        DrivingMetrics metrics,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(duration);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (string.IsNullOrWhiteSpace(policyNumber))
        {
            throw DomainException.Validation(TripErrorCodes.PolicyNumberRequired);
        }

        var normalizedPolicyNumber = policyNumber.Trim().ToUpperInvariant();
        if (normalizedPolicyNumber.Length > PolicyNumberMaxLength)
        {
            throw DomainException.Validation(TripErrorCodes.PolicyNumberTooLong, PolicyNumberMaxLength);
        }

        return new TripLog
        {
            Id = Guid.NewGuid(),
            PolicyNumber = normalizedPolicyNumber,
            Duration = duration,
            Metrics = metrics,
            Status = metrics.MaxSpeedMph > ReviewSpeedThresholdMph
                ? TripStatus.FlaggedForReview
                : TripStatus.Completed,
            RecordedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        };
    }
}
