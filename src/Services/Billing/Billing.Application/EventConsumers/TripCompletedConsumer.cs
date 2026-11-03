using Billing.Application.Interfaces;
using Billing.Domain;
using Billing.Domain.Entities;

using MassTransit;

using TelematicsInsurance.Contracts.Trips;

namespace Billing.Application.EventConsumers;

/// <summary>
/// Adds each completed trip to the policy's invoice for the month the trip ended.
/// </summary>
public sealed class TripCompletedConsumer(
    IBillingRepository billingRepository,
    TimeProvider timeProvider) : IConsumer<TripCompleted>
{
    public async Task Consume(ConsumeContext<TripCompleted> context)
    {
        var trip = context.Message;
        var billingPeriod = BillingPeriod.ForTripEndedAt(trip.TripEndedAtUtc);

        var invoice = await billingRepository.GetByPolicyAndPeriodAsync(
            trip.PolicyNumber,
            billingPeriod,
            context.CancellationToken);

        if (invoice is null)
        {
            invoice = MonthlyInvoice.CreateForPeriod(trip.PolicyNumber, billingPeriod, timeProvider);
            billingRepository.Add(invoice);
        }

        invoice.AccumulateTrip(
            trip.MilesDriven,
            trip.HardBrakeCount,
            trip.RapidAccelerationCount,
            trip.MaxSpeedMph,
            timeProvider);

        await billingRepository.SaveChangesAsync(context.CancellationToken);
    }
}
