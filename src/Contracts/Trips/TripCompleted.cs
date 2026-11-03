namespace TelematicsInsurance.Contracts.Trips;

/// <summary>
/// Published by the Telematics service after a trip is recorded.
/// </summary>
/// <remarks>
/// Integration contract: only add new optional members. Breaking changes require a new message type.
/// </remarks>
public sealed record TripCompleted(
    Guid TripId,
    string PolicyNumber,
    decimal MilesDriven,
    int HardBrakeCount,
    int RapidAccelerationCount,
    decimal MaxSpeedMph,
    DateTime TripEndedAtUtc);
