using Policy.Domain.Entities;
using Policy.Domain.Enums;
using Policy.Domain.Errors;
using Policy.Domain.ValueObjects;

namespace TelematicsInsurance.Tests;

public class PolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly FixedTimeProvider Clock = new(Now);

    // --- Vin ---

    [Fact]
    public void Vin_TrimsAndUppercases()
    {
        var vin = new Vin("  1hgcr2f83ha000000 ");

        Assert.Equal("1HGCR2F83HA000000", vin.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Vin_RejectsEmpty(string value)
    {
        var error = Assert.Throws<DomainException>(() => new Vin(value));

        Assert.Equal(PolicyErrorCodes.VinRequired, error.Code);
        Assert.Equal(DomainErrorKind.Validation, error.Kind);
    }

    [Theory]
    [InlineData("1HGCR2F83HA00000")]   // 16 characters
    [InlineData("1HGCR2F83HA0000000")] // 18 characters
    [InlineData("1HGCR2F83HI000000")]  // contains I
    [InlineData("1HGCR2F83HO000000")]  // contains O
    [InlineData("1HGCR2F83HQ000000")]  // contains Q
    [InlineData("1HGCR2F83H-000000")]  // contains a symbol
    public void Vin_RejectsInvalidFormat(string value)
    {
        var error = Assert.Throws<DomainException>(() => new Vin(value));

        Assert.Equal(PolicyErrorCodes.VinInvalidFormat, error.Code);
        Assert.Equal(value, Assert.Single(error.Arguments));
    }

    [Fact]
    public void Vin_EqualityIsByValue()
    {
        Assert.Equal(new Vin("1HGCR2F83HA000000"), new Vin("1hgcr2f83ha000000"));
    }

    // --- BaseRate ---

    [Fact]
    public void BaseRate_DefaultsToCad()
    {
        var rate = new BaseRate(150m);

        Assert.Equal(150m, rate.Amount);
        Assert.Equal("CAD", rate.Currency);
    }

    [Fact]
    public void BaseRate_AllowsZero()
    {
        Assert.Equal(0m, new BaseRate(0m).Amount);
    }

    [Fact]
    public void BaseRate_UppercasesCurrency()
    {
        Assert.Equal("USD", new BaseRate(10m, " usd ").Currency);
    }

    [Fact]
    public void BaseRate_RejectsNegativeAmount()
    {
        var error = Assert.Throws<DomainException>(() => new BaseRate(-0.01m));

        Assert.Equal(PolicyErrorCodes.BaseRateNegative, error.Code);
    }

    [Fact]
    public void BaseRate_RejectsMissingCurrency()
    {
        var error = Assert.Throws<DomainException>(() => new BaseRate(10m, " "));

        Assert.Equal(PolicyErrorCodes.CurrencyRequired, error.Code);
    }

    // --- InsurancePolicy creation ---

    [Fact]
    public void Create_StartsAsDraftWithGeneratedPolicyNumber()
    {
        var policy = CreatePolicy(customerName: "  Jane Doe ");

        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.Matches("^POL-[0-9A-F]{8}$", policy.PolicyNumber);
        Assert.Equal("Jane Doe", policy.CustomerName);
        Assert.NotEqual(Guid.Empty, policy.Id);
        Assert.Null(policy.UpdatedAtUtc);
    }

    [Fact]
    public void Create_GeneratesDifferentPolicyNumbers()
    {
        Assert.NotEqual(CreatePolicy().PolicyNumber, CreatePolicy().PolicyNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingCustomerName(string customerName)
    {
        var error = Assert.Throws<DomainException>(() => CreatePolicy(customerName: customerName));

        Assert.Equal(PolicyErrorCodes.CustomerNameRequired, error.Code);
    }

    [Fact]
    public void Create_RejectsCustomerNameOverMaxLength()
    {
        var tooLong = new string('a', InsurancePolicy.CustomerNameMaxLength + 1);

        var error = Assert.Throws<DomainException>(() => CreatePolicy(customerName: tooLong));

        Assert.Equal(PolicyErrorCodes.CustomerNameTooLong, error.Code);
        Assert.Equal(InsurancePolicy.CustomerNameMaxLength, Assert.Single(error.Arguments));
    }

    [Fact]
    public void Create_AllowsEffectiveDateFromStartOfYesterday()
    {
        var startOfYesterday = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc);

        var policy = CreatePolicy(effectiveDateUtc: startOfYesterday);

        Assert.Equal(startOfYesterday, policy.EffectiveDateUtc);
    }

    [Fact]
    public void Create_RejectsEffectiveDateBeforeYesterday()
    {
        var beforeYesterday = new DateTime(2026, 11, 1, 23, 59, 59, DateTimeKind.Utc);

        var error = Assert.Throws<DomainException>(() => CreatePolicy(effectiveDateUtc: beforeYesterday));

        Assert.Equal(PolicyErrorCodes.EffectiveDateInPast, error.Code);
    }

    [Fact]
    public void Create_TreatsUnspecifiedKindAsUtc()
    {
        var unspecified = new DateTime(2026, 11, 10, 8, 0, 0, DateTimeKind.Unspecified);

        var policy = CreatePolicy(effectiveDateUtc: unspecified);

        Assert.Equal(DateTimeKind.Utc, policy.EffectiveDateUtc.Kind);
        Assert.Equal(unspecified.Ticks, policy.EffectiveDateUtc.Ticks);
    }

    // --- InsurancePolicy lifecycle ---

    [Fact]
    public void Activate_FromDraft_SetsActive()
    {
        var policy = CreatePolicy();

        policy.Activate(Clock);

        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.Equal(Now.UtcDateTime, policy.UpdatedAtUtc);
    }

    [Fact]
    public void Suspend_FromDraft_IsRejected()
    {
        var policy = CreatePolicy();

        var error = Assert.Throws<DomainException>(() => policy.Suspend("Unpaid premium", Clock));

        Assert.Equal(PolicyErrorCodes.InvalidStatusTransition, error.Code);
        Assert.Equal(DomainErrorKind.Conflict, error.Kind);
        Assert.Equal([PolicyStatus.Draft, PolicyStatus.Suspended], error.Arguments);
    }

    [Fact]
    public void Suspend_ThenReactivate_IsAllowed()
    {
        var policy = CreatePolicy();
        policy.Activate(Clock);

        policy.Suspend("Unpaid premium", Clock);
        Assert.Equal(PolicyStatus.Suspended, policy.Status);

        policy.Activate(Clock);
        Assert.Equal(PolicyStatus.Active, policy.Status);
    }

    [Fact]
    public void Suspend_RequiresReason()
    {
        var policy = CreatePolicy();
        policy.Activate(Clock);

        var error = Assert.Throws<DomainException>(() => policy.Suspend(" ", Clock));

        Assert.Equal(PolicyErrorCodes.SuspendReasonRequired, error.Code);
    }

    [Fact]
    public void Cancel_Twice_IsRejected()
    {
        var policy = CreatePolicy();
        policy.Cancel(Clock);

        var error = Assert.Throws<DomainException>(() => policy.Cancel(Clock));

        Assert.Equal(PolicyErrorCodes.AlreadyCancelled, error.Code);
        Assert.Equal(DomainErrorKind.Conflict, error.Kind);
    }

    [Fact]
    public void Activate_AfterCancel_IsRejected()
    {
        var policy = CreatePolicy();
        policy.Cancel(Clock);

        var error = Assert.Throws<DomainException>(() => policy.Activate(Clock));

        Assert.Equal(PolicyErrorCodes.InvalidStatusTransition, error.Code);
    }

    [Fact]
    public void UpdateBaseRate_ChangesRate()
    {
        var policy = CreatePolicy();

        policy.UpdateBaseRate(new BaseRate(200m), Clock);

        Assert.Equal(new BaseRate(200m), policy.BaseMonthlyRate);
        Assert.Equal(Now.UtcDateTime, policy.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateBaseRate_OnCancelledPolicy_IsRejected()
    {
        var policy = CreatePolicy();
        policy.Cancel(Clock);

        var error = Assert.Throws<DomainException>(() => policy.UpdateBaseRate(new BaseRate(200m), Clock));

        Assert.Equal(PolicyErrorCodes.RateChangeOnCancelled, error.Code);
    }

    private static InsurancePolicy CreatePolicy(
        string customerName = "Jane Doe",
        DateTime? effectiveDateUtc = null) =>
        InsurancePolicy.Create(
            customerName,
            new Vin("1HGCR2F83HA000000"),
            new BaseRate(150m),
            effectiveDateUtc ?? Now.UtcDateTime,
            Clock);
}
