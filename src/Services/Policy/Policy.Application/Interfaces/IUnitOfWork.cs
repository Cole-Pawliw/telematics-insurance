namespace Policy.Application.Interfaces;

/// <summary>
/// Commits all pending changes, including outbox messages, in one transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
