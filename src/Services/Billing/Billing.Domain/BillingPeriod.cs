using System.Globalization;

namespace Billing.Domain;

/// <summary>
/// Billing periods are UTC calendar months formatted as "yyyy-MM".
/// </summary>
public static class BillingPeriod
{
    public const int Length = 7;

    /// <summary>
    /// Returns the billing period a trip belongs to, based on when the trip ended.
    /// </summary>
    public static string ForTripEndedAt(DateTime tripEndedAtUtc)
    {
        var utc = tripEndedAtUtc.Kind switch
        {
            DateTimeKind.Local => tripEndedAtUtc.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(tripEndedAtUtc, DateTimeKind.Utc),
            _ => tripEndedAtUtc,
        };

        return utc.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }
}
