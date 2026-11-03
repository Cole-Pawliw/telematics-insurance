using MassTransit;

using TelematicsInsurance.Contracts.Trips;

using Telematics.Application.Interfaces;
using Telematics.Domain.Entities;
using Telematics.Domain.ValueObjects;

namespace Telematics.Application.Commands;

public sealed class IngestTripCommandHandler(
    ITripRepository tripRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    TimeProvider timeProvider)
{
    public async Task<IngestTripResult> HandleAsync(IngestTripCommand command, CancellationToken cancellationToken)
    {
        var trip = TripLog.Record(
            command.PolicyNumber,
            new TripDuration(command.StartTimeUtc, command.EndTimeUtc),
            new DrivingMetrics(
                command.MilesDriven,
                command.HardBrakeCount,
                command.RapidAccelerationCount,
                command.MaxSpeedMph),
            timeProvider);

        tripRepository.Add(trip);

        // Written to the outbox table; only sent to RabbitMQ once the unit of work commits.
        await publishEndpoint.Publish(
            new TripCompleted(
                trip.Id,
                trip.PolicyNumber,
                trip.Metrics.MilesDriven,
                trip.Metrics.HardBrakeCount,
                trip.Metrics.RapidAccelerationCount,
                trip.Metrics.MaxSpeedMph,
                trip.Duration.EndTimeUtc),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new IngestTripResult(
            trip.Id,
            trip.PolicyNumber,
            trip.Status.ToString(),
            trip.Duration.Elapsed.TotalMinutes);
    }
}
