using Telematics.Domain.Errors;

namespace Telematics.Domain.ValueObjects;

/// <summary>
/// Driving behaviour measured during a single trip.
/// </summary>
public sealed record DrivingMetrics
{
    public DrivingMetrics(
        decimal milesDriven,
        int hardBrakeCount,
        int rapidAccelerationCount,
        decimal maxSpeedMph)
    {
        if (milesDriven < 0)
        {
            throw DomainException.Validation(TripErrorCodes.MilesDrivenNegative);
        }

        if (hardBrakeCount < 0)
        {
            throw DomainException.Validation(TripErrorCodes.HardBrakeCountNegative);
        }

        if (rapidAccelerationCount < 0)
        {
            throw DomainException.Validation(TripErrorCodes.RapidAccelerationCountNegative);
        }

        if (maxSpeedMph < 0)
        {
            throw DomainException.Validation(TripErrorCodes.MaxSpeedNegative);
        }

        MilesDriven = milesDriven;
        HardBrakeCount = hardBrakeCount;
        RapidAccelerationCount = rapidAccelerationCount;
        MaxSpeedMph = maxSpeedMph;
    }

    public decimal MilesDriven { get; }

    public int HardBrakeCount { get; }

    public int RapidAccelerationCount { get; }

    public decimal MaxSpeedMph { get; }
}
