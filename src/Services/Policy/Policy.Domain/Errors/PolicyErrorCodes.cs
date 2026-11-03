namespace Policy.Domain.Errors;

/// <summary>
/// Error codes raised by the Policy domain. Each code has a matching message in the API's DomainExceptionHandler.
/// </summary>
public static class PolicyErrorCodes
{
    public const string CustomerNameRequired = "Policy.CustomerNameRequired";
    public const string CustomerNameTooLong = "Policy.CustomerNameTooLong";
    public const string EffectiveDateInPast = "Policy.EffectiveDateInPast";
    public const string VinRequired = "Policy.VinRequired";
    public const string VinInvalidFormat = "Policy.VinInvalidFormat";
    public const string BaseRateNegative = "Policy.BaseRateNegative";
    public const string CurrencyRequired = "Policy.CurrencyRequired";
    public const string SuspendReasonRequired = "Policy.SuspendReasonRequired";
    public const string InvalidStatusTransition = "Policy.InvalidStatusTransition";
    public const string AlreadyCancelled = "Policy.AlreadyCancelled";
    public const string RateChangeOnCancelled = "Policy.RateChangeOnCancelled";
}
