using System.Text.RegularExpressions;

using Policy.Domain.Errors;

namespace Policy.Domain.ValueObjects;

/// <summary>
/// A 17-character vehicle identification number. Letters I, O and Q are not allowed.
/// </summary>
public sealed partial record Vin
{
    public const int Length = 17;

    public Vin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw DomainException.Validation(PolicyErrorCodes.VinRequired);
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (!VinFormat().IsMatch(normalized))
        {
            throw DomainException.Validation(PolicyErrorCodes.VinInvalidFormat, value);
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-HJ-NPR-Z0-9]{17}$")]
    private static partial Regex VinFormat();
}
