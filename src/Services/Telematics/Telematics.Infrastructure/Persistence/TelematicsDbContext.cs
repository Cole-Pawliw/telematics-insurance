using MassTransit;

using Microsoft.EntityFrameworkCore;

using Telematics.Application.Interfaces;
using Telematics.Domain.Entities;

namespace Telematics.Infrastructure.Persistence;

public sealed class TelematicsDbContext(DbContextOptions<TelematicsDbContext> options) : DbContext(options), IUnitOfWork
{
    public const string Schema = "telematics";

    public DbSet<TripLog> TripLogs => Set<TripLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelematicsDbContext).Assembly);

        // Tables used by the MassTransit transactional outbox.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
