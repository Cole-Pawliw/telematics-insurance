using Policy.Domain.Enums;
using Policy.Domain.Errors;
using Policy.Domain.ValueObjects;

namespace Policy.Domain.Entities;

/// <summary>
/// An insurance policy covering one customer and one vehicle.
/// </summary>
public sealed class InsurancePolicy
{
    public const int CustomerNameMaxLength = 100;
    public const int PolicyNumberMaxLength = 32;

    // Used by EF Core when loading from the database.
    private InsurancePolicy()
    {
    }

    public Guid Id { get; private set; }

    public string PolicyNumber { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;

    public Vin VehicleVin { get; private set; } = null!;

    public BaseRate BaseMonthlyRate { get; private set; } = null!;

    public PolicyStatus Status { get; private set; }

    public DateTime EffectiveDateUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public static InsurancePolicy Create(
        string customerName,
        Vin vehicleVin,
        BaseRate baseMonthlyRate,
        DateTime effectiveDateUtc,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(vehicleVin);
        ArgumentNullException.ThrowIfNull(baseMonthlyRate);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw DomainException.Validation(PolicyErrorCodes.CustomerNameRequired);
        }

        var trimmedName = customerName.Trim();
        if (trimmedName.Length > CustomerNameMaxLength)
        {
            throw DomainException.Validation(PolicyErrorCodes.CustomerNameTooLong, CustomerNameMaxLength);
        }

        // One day of tolerance so "today" is accepted whatever the caller's time zone.
        var effectiveUtc = ToUtc(effectiveDateUtc);
        var earliestAllowed = timeProvider.GetUtcNow().UtcDateTime.Date.AddDays(-1);
        if (effectiveUtc < earliestAllowed)
        {
            throw DomainException.Validation(PolicyErrorCodes.EffectiveDateInPast);
        }

        return new InsurancePolicy
        {
            Id = Guid.NewGuid(),
            PolicyNumber = GeneratePolicyNumber(),
            CustomerName = trimmedName,
            VehicleVin = vehicleVin,
            BaseMonthlyRate = baseMonthlyRate,
            Status = PolicyStatus.Draft,
            EffectiveDateUtc = effectiveUtc,
        };
    }

    public void Activate(TimeProvider timeProvider)
    {
        EnsureCanTransitionTo(PolicyStatus.Active);
        SetStatus(PolicyStatus.Active, timeProvider);
    }

    public void Suspend(string reason, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw DomainException.Validation(PolicyErrorCodes.SuspendReasonRequired);
        }

        EnsureCanTransitionTo(PolicyStatus.Suspended);
        SetStatus(PolicyStatus.Suspended, timeProvider);
    }

    public void Cancel(TimeProvider timeProvider)
    {
        if (Status == PolicyStatus.Cancelled)
        {
            throw DomainException.Conflict(PolicyErrorCodes.AlreadyCancelled);
        }

        SetStatus(PolicyStatus.Cancelled, timeProvider);
    }

    public void UpdateBaseRate(BaseRate newRate, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(newRate);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (Status == PolicyStatus.Cancelled)
        {
            throw DomainException.Conflict(PolicyErrorCodes.RateChangeOnCancelled);
        }

        BaseMonthlyRate = newRate;
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    }

    private static string GeneratePolicyNumber() =>
        $"POL-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value,
    };

    private void EnsureCanTransitionTo(PolicyStatus target)
    {
        var allowed = (Status, target) switch
        {
            (PolicyStatus.Draft, PolicyStatus.Active) => true,
            (PolicyStatus.Active, PolicyStatus.Suspended) => true,
            (PolicyStatus.Suspended, PolicyStatus.Active) => true,
            _ => false,
        };

        if (!allowed)
        {
            throw DomainException.Conflict(PolicyErrorCodes.InvalidStatusTransition, Status, target);
        }
    }

    private void SetStatus(PolicyStatus status, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        Status = status;
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    }
}
