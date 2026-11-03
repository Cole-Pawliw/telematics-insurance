using Telematics.Domain.Entities;

namespace Telematics.Application.Interfaces;

public interface ITripRepository
{
    void Add(TripLog trip);

    /// <summary>Returns the policy's trips, most recently ended first.</summary>
    Task<IReadOnlyList<TripLog>> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken);
}
