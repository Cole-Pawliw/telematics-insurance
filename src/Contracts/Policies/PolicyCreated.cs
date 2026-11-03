namespace TelematicsInsurance.Contracts.Policies;

/// <summary>
/// Published by the Policy service after a policy is created.
/// </summary>
/// <remarks>
/// Integration contract: only add new optional members. Breaking changes require a new message type.
/// </remarks>
public sealed record PolicyCreated(
    Guid PolicyId,
    string PolicyNumber,
    string Vin,
    decimal BaseMonthlyRate,
    string Currency,
    DateTime EffectiveDateUtc);
