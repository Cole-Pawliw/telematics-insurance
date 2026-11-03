using MassTransit;

using Policy.Application.Interfaces;
using Policy.Domain.Entities;
using Policy.Domain.ValueObjects;

using TelematicsInsurance.Contracts.Policies;

namespace Policy.Application.Commands;

public sealed class CreatePolicyCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    TimeProvider timeProvider)
{
    public async Task<CreatePolicyResult> HandleAsync(CreatePolicyCommand command, CancellationToken cancellationToken)
    {
        var policy = InsurancePolicy.Create(
            command.CustomerName,
            new Vin(command.Vin),
            new BaseRate(command.BaseMonthlyRate),
            command.EffectiveDateUtc,
            timeProvider);

        policyRepository.Add(policy);

        // Written to the outbox table; only sent to RabbitMQ once the unit of work commits.
        await publishEndpoint.Publish(
            new PolicyCreated(
                policy.Id,
                policy.PolicyNumber,
                policy.VehicleVin.Value,
                policy.BaseMonthlyRate.Amount,
                policy.BaseMonthlyRate.Currency,
                policy.EffectiveDateUtc),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreatePolicyResult(policy.Id, policy.PolicyNumber, policy.Status.ToString());
    }
}
