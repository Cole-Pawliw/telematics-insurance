using Telematics.Application.Commands;
using Telematics.Application.Interfaces;

namespace Telematics.Api.Endpoints;

public static class TelematicsEndpoints
{
    public static void MapTelematicsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trips").WithTags("Telematics");

        group.MapPost("/", async (IngestTripCommand command, IngestTripCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return TypedResults.Created($"/api/trips/{result.TripId}", result);
        })
        .WithName("IngestTrip")
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/policy/{policyNumber}", async (string policyNumber, ITripRepository tripRepository, CancellationToken ct) =>
        {
            var trips = await tripRepository.GetByPolicyNumberAsync(policyNumber, ct);
            return TypedResults.Ok(trips.Select(TripResponse.From).ToList());
        })
        .WithName("GetTripsByPolicy");
    }
}
