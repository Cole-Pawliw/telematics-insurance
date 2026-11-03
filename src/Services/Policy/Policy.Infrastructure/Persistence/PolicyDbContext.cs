using MassTransit;

using Microsoft.EntityFrameworkCore;

using Policy.Application.Interfaces;
using Policy.Domain.Entities;

namespace Policy.Infrastructure.Persistence;

public sealed class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options), IUnitOfWork
{
    public const string Schema = "policy";

    public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PolicyDbContext).Assembly);

        // Tables used by the MassTransit transactional outbox.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
