using Policy.Domain.Entities;

namespace Policy.Application.Interfaces;

public interface IPolicyRepository
{
    void Add(InsurancePolicy policy);
}
