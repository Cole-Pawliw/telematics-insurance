namespace Billing.Domain.Entities;

/// <summary>
/// A policy's driving summary for one billing period, with the resulting safety score
/// and premium adjustment.
/// </summary>
public sealed class MonthlyInvoice
{
    public const int PolicyNumberMaxLength = 32;
    public const int PerfectSafetyScore = 100;

    private const int HardBrakePenalty = 3;
    private const int RapidAccelerationPenalty = 2;
    private const decimal SpeedingThresholdMph = 80m;
    private const decimal SpeedingPenaltyPerMph = 0.5m;

    // Used by EF Core when loading from the database.
    private MonthlyInvoice()
    {
    }

    public Guid Id { get; private set; }

    public string PolicyNumber { get; private set; } = string.Empty;

    /// <summary>UTC calendar month, formatted "yyyy-MM".</summary>
    public string BillingPeriod { get; private set; } = string.Empty;

    public decimal TotalMilesDriven { get; private set; }

    /// <summary>0 to 100. Starts at 100 each period and only decreases.</summary>
    public int CalculatedSafetyScore { get; private set; }

    /// <summary>Percentage applied to the base rate, e.g. -15.00 for a 15% discount.</summary>
    public decimal PremiumAdjustmentPercentage { get; private set; }

    public DateTime LastUpdatedUtc { get; private set; }

    public static MonthlyInvoice CreateForPeriod(string policyNumber, string billingPeriod, TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(billingPeriod);
        ArgumentNullException.ThrowIfNull(timeProvider);

        return new MonthlyInvoice
        {
            Id = Guid.NewGuid(),
            PolicyNumber = policyNumber.Trim().ToUpperInvariant(),
            BillingPeriod = billingPeriod,
            TotalMilesDriven = 0m,
            CalculatedSafetyScore = PerfectSafetyScore,
            PremiumAdjustmentPercentage = 0m,
            LastUpdatedUtc = timeProvider.GetUtcNow().UtcDateTime,
        };
    }

    /// <summary>
    /// Premium tier for a safety score: good drivers get a discount, risky drivers a surcharge.
    /// </summary>
    public static decimal PremiumAdjustmentFor(int safetyScore) => safetyScore switch
    {
        >= 90 => -15.00m,
        >= 75 => -5.00m,
        >= 60 => 0.00m,
        >= 45 => 10.00m,
        _ => 25.00m,
    };

    /// <summary>
    /// Adds a trip's miles and deducts points for risky driving, then recalculates the premium tier.
    /// </summary>
    public void AccumulateTrip(
        decimal milesDriven,
        int hardBrakeCount,
        int rapidAccelerationCount,
        decimal maxSpeedMph,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        TotalMilesDriven += milesDriven;

        var deductions = (hardBrakeCount * HardBrakePenalty) + (rapidAccelerationCount * RapidAccelerationPenalty);
        if (maxSpeedMph > SpeedingThresholdMph)
        {
            // Fractional points are dropped, e.g. 85 mph costs 2 points, not 2.5.
            deductions += (int)((maxSpeedMph - SpeedingThresholdMph) * SpeedingPenaltyPerMph);
        }

        CalculatedSafetyScore = Math.Max(0, CalculatedSafetyScore - deductions);
        PremiumAdjustmentPercentage = PremiumAdjustmentFor(CalculatedSafetyScore);
        LastUpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
    }
}
