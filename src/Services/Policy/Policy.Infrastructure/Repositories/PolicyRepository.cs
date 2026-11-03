using Policy.Application.Interfaces;
using Policy.Domain.Entities;
using Policy.Infrastructure.Persistence;

namespace Policy.Infrastructure.Repositories;

public sealed class PolicyRepository(PolicyDbContext dbContext) : IPolicyRepository
{
    public void Add(InsurancePolicy policy) => dbContext.Policies.Add(policy);
}
