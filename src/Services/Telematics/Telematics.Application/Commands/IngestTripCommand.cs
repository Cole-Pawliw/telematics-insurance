namespace Telematics.Application.Commands;

public sealed record IngestTripCommand(
    string PolicyNumber,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    decimal MilesDriven,
    int HardBrakeCount,
    int RapidAccelerationCount,
    decimal MaxSpeedMph);

public sealed record IngestTripResult(
    Guid TripId,
    string PolicyNumber,
    string Status,
    double DurationMinutes);
