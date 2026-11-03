using Billing.Application.Interfaces;

namespace Billing.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing").WithTags("Billing");

        group.MapGet("/policy/{policyNumber}", async (string policyNumber, IBillingRepository billingRepository, CancellationToken ct) =>
        {
            var invoices = await billingRepository.GetByPolicyNumberAsync(policyNumber, ct);
            return TypedResults.Ok(invoices.Select(InvoiceResponse.From).ToList());
        })
        .WithName("GetBillingByPolicy");
    }
}
