using Telematics.Domain.Entities;

namespace Telematics.Api.Endpoints;

public sealed record TripResponse(
    Guid Id,
    string PolicyNumber,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    decimal MilesDriven,
    int HardBrakeCount,
    int RapidAccelerationCount,
    decimal MaxSpeedMph,
    string Status,
    DateTime RecordedAtUtc)
{
    public static TripResponse From(TripLog trip) => new(
        trip.Id,
        trip.PolicyNumber,
        trip.Duration.StartTimeUtc,
        trip.Duration.EndTimeUtc,
        trip.Metrics.MilesDriven,
        trip.Metrics.HardBrakeCount,
        trip.Metrics.RapidAccelerationCount,
        trip.Metrics.MaxSpeedMph,
        trip.Status.ToString(),
        trip.RecordedAtUtc);
}
