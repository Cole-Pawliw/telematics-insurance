using System.Globalization;

using Billing.Domain;
using Billing.Domain.Entities;

namespace TelematicsInsurance.Tests;

public class BillingTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly FixedTimeProvider Clock = new(Now);

    // --- BillingPeriod ---

    [Fact]
    public void BillingPeriod_UsesMonthTheTripEnded()
    {
        // Trip ended late on Oct 31 (UTC); it belongs to October even if uploaded in November.
        var endedAt = new DateTime(2026, 10, 31, 23, 30, 0, DateTimeKind.Utc);

        Assert.Equal("2026-10", BillingPeriod.ForTripEndedAt(endedAt));
    }

    [Fact]
    public void BillingPeriod_StartOfMonthIsNewPeriod()
    {
        var endedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal("2026-11", BillingPeriod.ForTripEndedAt(endedAt));
    }

    [Fact]
    public void BillingPeriod_TreatsUnspecifiedKindAsUtc()
    {
        var endedAt = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Unspecified);

        Assert.Equal("2026-12", BillingPeriod.ForTripEndedAt(endedAt));
    }

    // --- MonthlyInvoice ---

    [Fact]
    public void CreateForPeriod_StartsWithPerfectScore()
    {
        var invoice = MonthlyInvoice.CreateForPeriod(" pol-1a2b3c4d ", "2026-10", Clock);

        Assert.Equal("POL-1A2B3C4D", invoice.PolicyNumber);
        Assert.Equal("2026-10", invoice.BillingPeriod);
        Assert.Equal(100, invoice.CalculatedSafetyScore);
        Assert.Equal(0m, invoice.TotalMilesDriven);
        Assert.Equal(0m, invoice.PremiumAdjustmentPercentage);
    }

    [Fact]
    public void AccumulateTrip_DeductsThreePointsPerHardBrake()
    {
        var invoice = CreateInvoice();

        invoice.AccumulateTrip(10m, hardBrakeCount: 4, rapidAccelerationCount: 0, maxSpeedMph: 60m, Clock);

        Assert.Equal(88, invoice.CalculatedSafetyScore);
    }

    [Fact]
    public void AccumulateTrip_CombinesAllDeductions()
    {
        var invoice = CreateInvoice();

        // 1 hard brake (3) + 2 rapid accelerations (4) + 85 mph (5 over x 0.5 = 2.5, truncated to 2) = 9
        invoice.AccumulateTrip(10m, hardBrakeCount: 1, rapidAccelerationCount: 2, maxSpeedMph: 85m, Clock);

        Assert.Equal(91, invoice.CalculatedSafetyScore);
        Assert.Equal(-15.00m, invoice.PremiumAdjustmentPercentage);
        Assert.Equal(Now.UtcDateTime, invoice.LastUpdatedUtc);
    }

    [Theory]
    [InlineData("80", 100)]
    [InlineData("81", 100)]
    [InlineData("82", 99)]
    [InlineData("120", 80)]
    public void AccumulateTrip_DeductsHalfPointPerMphOver80(string maxSpeedMph, int expectedScore)
    {
        var invoice = CreateInvoice();

        invoice.AccumulateTrip(10m, 0, 0, decimal.Parse(maxSpeedMph, CultureInfo.InvariantCulture), Clock);

        Assert.Equal(expectedScore, invoice.CalculatedSafetyScore);
    }

    [Fact]
    public void AccumulateTrip_ScoreNeverDropsBelowZero()
    {
        var invoice = CreateInvoice();

        invoice.AccumulateTrip(10m, hardBrakeCount: 50, rapidAccelerationCount: 0, maxSpeedMph: 60m, Clock);

        Assert.Equal(0, invoice.CalculatedSafetyScore);
        Assert.Equal(25.00m, invoice.PremiumAdjustmentPercentage);
    }

    [Fact]
    public void AccumulateTrip_AddsMilesAndKeepsDeductingAcrossTrips()
    {
        var invoice = CreateInvoice();

        invoice.AccumulateTrip(25.5m, 1, 0, 60m, Clock);
        invoice.AccumulateTrip(10.25m, 1, 0, 60m, Clock);

        Assert.Equal(35.75m, invoice.TotalMilesDriven);
        Assert.Equal(94, invoice.CalculatedSafetyScore);
    }

    [Theory]
    [InlineData(100, "-15.00")]
    [InlineData(90, "-15.00")]
    [InlineData(89, "-5.00")]
    [InlineData(75, "-5.00")]
    [InlineData(74, "0.00")]
    [InlineData(60, "0.00")]
    [InlineData(59, "10.00")]
    [InlineData(45, "10.00")]
    [InlineData(44, "25.00")]
    [InlineData(0, "25.00")]
    public void PremiumAdjustmentFor_UsesTierBoundaries(int safetyScore, string expectedAdjustment)
    {
        var expected = decimal.Parse(expectedAdjustment, CultureInfo.InvariantCulture);

        Assert.Equal(expected, MonthlyInvoice.PremiumAdjustmentFor(safetyScore));
    }

    private static MonthlyInvoice CreateInvoice() =>
        MonthlyInvoice.CreateForPeriod("POL-1A2B3C4D", "2026-10", Clock);
}
