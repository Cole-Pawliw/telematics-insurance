namespace Policy.Application.Commands;

public sealed record CreatePolicyCommand(
    string CustomerName,
    string Vin,
    decimal BaseMonthlyRate,
    DateTime EffectiveDateUtc);

public sealed record CreatePolicyResult(
    Guid PolicyId,
    string PolicyNumber,
    string Status);
