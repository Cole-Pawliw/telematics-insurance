using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Policy.Domain.Entities;
using Policy.Domain.ValueObjects;

namespace Policy.Infrastructure.Persistence.Configurations;

internal sealed class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PolicyNumber).HasMaxLength(InsurancePolicy.PolicyNumberMaxLength);
        builder.HasIndex(p => p.PolicyNumber).IsUnique();

        builder.Property(p => p.CustomerName).HasMaxLength(InsurancePolicy.CustomerNameMaxLength);

        // Value objects are stored as columns on the Policies table.
        builder.ComplexProperty(p => p.VehicleVin, vin =>
            vin.Property(v => v.Value).HasColumnName("VehicleVin").HasMaxLength(Vin.Length));

        builder.ComplexProperty(p => p.BaseMonthlyRate, rate =>
        {
            rate.Property(r => r.Amount).HasColumnName("BaseMonthlyRate").HasPrecision(10, 2);
            rate.Property(r => r.Currency).HasColumnName("Currency").HasMaxLength(BaseRate.CurrencyLength);
        });
    }
}
