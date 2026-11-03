using Microsoft.EntityFrameworkCore;

using Telematics.Application.Interfaces;
using Telematics.Domain.Entities;
using Telematics.Infrastructure.Persistence;

namespace Telematics.Infrastructure.Repositories;

public sealed class TripRepository(TelematicsDbContext dbContext) : ITripRepository
{
    public void Add(TripLog trip) => dbContext.TripLogs.Add(trip);

    public async Task<IReadOnlyList<TripLog>> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var normalized = policyNumber.Trim().ToUpperInvariant();

        return await dbContext.TripLogs
            .AsNoTracking()
            .Where(t => t.PolicyNumber == normalized)
            .OrderByDescending(t => t.Duration.EndTimeUtc)
            .ToListAsync(cancellationToken);
    }
}
