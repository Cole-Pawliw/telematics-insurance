using Billing.Domain.Entities;

namespace Billing.Api.Endpoints;

public sealed record InvoiceResponse(
    Guid Id,
    string PolicyNumber,
    string BillingPeriod,
    decimal TotalMilesDriven,
    int CalculatedSafetyScore,
    decimal PremiumAdjustmentPercentage,
    DateTime LastUpdatedUtc)
{
    public static InvoiceResponse From(MonthlyInvoice invoice) => new(
        invoice.Id,
        invoice.PolicyNumber,
        invoice.BillingPeriod,
        invoice.TotalMilesDriven,
        invoice.CalculatedSafetyScore,
        invoice.PremiumAdjustmentPercentage,
        invoice.LastUpdatedUtc);
}
