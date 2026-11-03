using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Telematics.Domain.Entities;

namespace Telematics.Infrastructure.Persistence.Configurations;

internal sealed class TripLogConfiguration : IEntityTypeConfiguration<TripLog>
{
    public void Configure(EntityTypeBuilder<TripLog> builder)
    {
        builder.ToTable("TripLogs");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PolicyNumber).HasMaxLength(TripLog.PolicyNumberMaxLength);
        builder.HasIndex(t => t.PolicyNumber);

        // Value objects are stored as columns on the TripLogs table.
        builder.ComplexProperty(t => t.Duration, duration =>
        {
            duration.Property(d => d.StartTimeUtc).HasColumnName("StartTimeUtc");
            duration.Property(d => d.EndTimeUtc).HasColumnName("EndTimeUtc");
        });

        builder.ComplexProperty(t => t.Metrics, metrics =>
        {
            metrics.Property(m => m.MilesDriven).HasColumnName("MilesDriven").HasPrecision(8, 2);
            metrics.Property(m => m.HardBrakeCount).HasColumnName("HardBrakeCount");
            metrics.Property(m => m.RapidAccelerationCount).HasColumnName("RapidAccelerationCount");
            metrics.Property(m => m.MaxSpeedMph).HasColumnName("MaxSpeedMph").HasPrecision(5, 2);
        });
    }
}
