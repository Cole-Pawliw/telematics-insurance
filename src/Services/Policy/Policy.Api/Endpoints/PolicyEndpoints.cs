using Policy.Application.Commands;

namespace Policy.Api.Endpoints;

public static class PolicyEndpoints
{
    public static void MapPolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/policies").WithTags("Policies");

        group.MapPost("/", async (CreatePolicyCommand command, CreatePolicyCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return TypedResults.Created($"/api/policies/{result.PolicyId}", result);
        })
        .WithName("CreatePolicy")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
