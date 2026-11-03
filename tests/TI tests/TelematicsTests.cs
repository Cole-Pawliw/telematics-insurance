using System.Globalization;

using Telematics.Domain.Entities;
using Telematics.Domain.Enums;
using Telematics.Domain.Errors;
using Telematics.Domain.ValueObjects;

namespace TelematicsInsurance.Tests;

public class TelematicsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 0, 10, 0, TimeSpan.Zero);
    private static readonly FixedTimeProvider Clock = new(Now);
    private static readonly DateTime Start = new(2026, 10, 31, 23, 0, 0, DateTimeKind.Utc);

    // --- TripDuration ---

    [Fact]
    public void TripDuration_CalculatesElapsedTime()
    {
        var duration = new TripDuration(Start, Start.AddMinutes(45));

        Assert.Equal(TimeSpan.FromMinutes(45), duration.Elapsed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TripDuration_RejectsEndNotAfterStart(int minutesAfterStart)
    {
        var error = Assert.Throws<DomainException>(() => new TripDuration(Start, Start.AddMinutes(minutesAfterStart)));

        Assert.Equal(TripErrorCodes.EndNotAfterStart, error.Code);
    }

    [Fact]
    public void TripDuration_ConvertsLocalTimesToUtc()
    {
        var localStart = new DateTime(2026, 10, 31, 17, 0, 0, DateTimeKind.Local);

        var duration = new TripDuration(localStart, localStart.AddHours(1));

        Assert.Equal(DateTimeKind.Utc, duration.StartTimeUtc.Kind);
        Assert.Equal(localStart.ToUniversalTime(), duration.StartTimeUtc);
    }

    [Fact]
    public void TripDuration_TreatsUnspecifiedKindAsUtc()
    {
        var unspecified = new DateTime(2026, 10, 31, 23, 0, 0, DateTimeKind.Unspecified);

        var duration = new TripDuration(unspecified, unspecified.AddHours(1));

        Assert.Equal(DateTimeKind.Utc, duration.EndTimeUtc.Kind);
        Assert.Equal(unspecified.AddHours(1).Ticks, duration.EndTimeUtc.Ticks);
    }

    // --- DrivingMetrics ---

    [Fact]
    public void DrivingMetrics_AllowsZeroValues()
    {
        var metrics = new DrivingMetrics(0m, 0, 0, 0m);

        Assert.Equal(0m, metrics.MilesDriven);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0, TripErrorCodes.MilesDrivenNegative)]
    [InlineData(1, -1, 0, 0, TripErrorCodes.HardBrakeCountNegative)]
    [InlineData(1, 0, -1, 0, TripErrorCodes.RapidAccelerationCountNegative)]
    [InlineData(1, 0, 0, -1, TripErrorCodes.MaxSpeedNegative)]
    public void DrivingMetrics_RejectsNegativeValues(
        int milesDriven,
        int hardBrakeCount,
        int rapidAccelerationCount,
        int maxSpeedMph,
        string expectedCode)
    {
        var error = Assert.Throws<DomainException>(
            () => new DrivingMetrics(milesDriven, hardBrakeCount, rapidAccelerationCount, maxSpeedMph));

        Assert.Equal(expectedCode, error.Code);
    }

    // --- TripLog ---

    [Fact]
    public void Record_NormalizesPolicyNumberAndSetsRecordedTime()
    {
        var trip = RecordTrip(policyNumber: "  pol-1a2b3c4d ");

        Assert.Equal("POL-1A2B3C4D", trip.PolicyNumber);
        Assert.Equal(Now.UtcDateTime, trip.RecordedAtUtc);
        Assert.NotEqual(Guid.Empty, trip.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Record_RejectsMissingPolicyNumber(string policyNumber)
    {
        var error = Assert.Throws<DomainException>(() => RecordTrip(policyNumber: policyNumber));

        Assert.Equal(TripErrorCodes.PolicyNumberRequired, error.Code);
    }

    [Fact]
    public void Record_RejectsPolicyNumberOverMaxLength()
    {
        var tooLong = new string('A', TripLog.PolicyNumberMaxLength + 1);

        var error = Assert.Throws<DomainException>(() => RecordTrip(policyNumber: tooLong));

        Assert.Equal(TripErrorCodes.PolicyNumberTooLong, error.Code);
    }

    [Theory]
    [InlineData("72", TripStatus.Completed)]
    [InlineData("100", TripStatus.Completed)]
    [InlineData("100.01", TripStatus.FlaggedForReview)]
    public void Record_FlagsTripsOver100Mph(string maxSpeedMph, TripStatus expectedStatus)
    {
        var trip = RecordTrip(maxSpeedMph: decimal.Parse(maxSpeedMph, CultureInfo.InvariantCulture));

        Assert.Equal(expectedStatus, trip.Status);
    }

    private static TripLog RecordTrip(string policyNumber = "POL-1A2B3C4D", decimal maxSpeedMph = 72m)
    {
        var end = new DateTime(2026, 10, 31, 23, 30, 0, DateTimeKind.Utc);

        return TripLog.Record(
            policyNumber,
            new TripDuration(end.AddMinutes(-45), end),
            new DrivingMetrics(25.5m, 1, 0, maxSpeedMph),
            Clock);
    }
}
