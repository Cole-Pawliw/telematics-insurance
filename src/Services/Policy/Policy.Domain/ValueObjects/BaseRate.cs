using Policy.Domain.Errors;

namespace Policy.Domain.ValueObjects;

/// <summary>
/// The monthly premium before any telematics adjustment.
/// </summary>
public sealed record BaseRate
{
    public const string DefaultCurrency = "CAD";
    public const int CurrencyLength = 3;

    public BaseRate(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
        {
            throw DomainException.Validation(PolicyErrorCodes.BaseRateNegative);
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw DomainException.Validation(PolicyErrorCodes.CurrencyRequired);
        }

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; }

    public string Currency { get; }
}
