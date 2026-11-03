using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

public interface IBillingRepository
{
    Task<MonthlyInvoice?> GetByPolicyAndPeriodAsync(string policyNumber, string billingPeriod, CancellationToken cancellationToken);

    /// <summary>Returns the policy's invoices, newest billing period first.</summary>
    Task<IReadOnlyList<MonthlyInvoice>> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken);

    void Add(MonthlyInvoice invoice);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
