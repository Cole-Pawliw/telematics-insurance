namespace Telematics.Domain.Errors;

/// <summary>
/// Error codes raised by the Telematics domain. Each code has a matching message in the API's DomainExceptionHandler.
/// </summary>
public static class TripErrorCodes
{
    public const string PolicyNumberRequired = "Trip.PolicyNumberRequired";
    public const string PolicyNumberTooLong = "Trip.PolicyNumberTooLong";
    public const string EndNotAfterStart = "Trip.EndNotAfterStart";
    public const string MilesDrivenNegative = "Trip.MilesDrivenNegative";
    public const string HardBrakeCountNegative = "Trip.HardBrakeCountNegative";
    public const string RapidAccelerationCountNegative = "Trip.RapidAccelerationCountNegative";
    public const string MaxSpeedNegative = "Trip.MaxSpeedNegative";
}
