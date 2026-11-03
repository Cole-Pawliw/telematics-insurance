using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

public sealed class BillingRepository(BillingDbContext dbContext) : IBillingRepository
{
    public Task<MonthlyInvoice?> GetByPolicyAndPeriodAsync(string policyNumber, string billingPeriod, CancellationToken cancellationToken)
    {
        var normalized = policyNumber.Trim().ToUpperInvariant();

        return dbContext.MonthlyInvoices.SingleOrDefaultAsync(
            i => i.PolicyNumber == normalized && i.BillingPeriod == billingPeriod,
            cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyInvoice>> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var normalized = policyNumber.Trim().ToUpperInvariant();

        return await dbContext.MonthlyInvoices
            .AsNoTracking()
            .Where(i => i.PolicyNumber == normalized)
            .OrderByDescending(i => i.BillingPeriod)
            .ToListAsync(cancellationToken);
    }

    public void Add(MonthlyInvoice invoice) => dbContext.MonthlyInvoices.Add(invoice);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
