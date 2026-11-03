using Billing.Domain;
using Billing.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

internal sealed class MonthlyInvoiceConfiguration : IEntityTypeConfiguration<MonthlyInvoice>
{
    public void Configure(EntityTypeBuilder<MonthlyInvoice> builder)
    {
        builder.ToTable("MonthlyInvoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.PolicyNumber).HasMaxLength(MonthlyInvoice.PolicyNumberMaxLength);
        builder.Property(i => i.BillingPeriod).HasMaxLength(BillingPeriod.Length);
        builder.HasIndex(i => new { i.PolicyNumber, i.BillingPeriod }).IsUnique();

        builder.Property(i => i.TotalMilesDriven).HasPrecision(10, 2);
        builder.Property(i => i.PremiumAdjustmentPercentage).HasPrecision(5, 2);

        // Maps to Postgres' built-in xmin column so concurrent updates to one invoice are detected.
        builder.Property<uint>("RowVersion").IsRowVersion();
    }
}
