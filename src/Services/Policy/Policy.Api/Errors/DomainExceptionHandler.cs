using Microsoft.AspNetCore.Diagnostics;

using Policy.Domain.Errors;

namespace Policy.Api.Errors;

/// <summary>
/// Turns broken domain rules into 400/409 problem responses with a readable message.
/// </summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        [PolicyErrorCodes.CustomerNameRequired] = "Customer name is required.",
        [PolicyErrorCodes.CustomerNameTooLong] = "Customer name cannot be longer than {0} characters.",
        [PolicyErrorCodes.EffectiveDateInPast] = "Effective date cannot be in the past.",
        [PolicyErrorCodes.VinRequired] = "VIN is required.",
        [PolicyErrorCodes.VinInvalidFormat] = "Invalid VIN '{0}'. A VIN is 17 letters or digits and cannot contain I, O or Q.",
        [PolicyErrorCodes.BaseRateNegative] = "Base monthly rate cannot be negative.",
        [PolicyErrorCodes.CurrencyRequired] = "Currency is required.",
        [PolicyErrorCodes.SuspendReasonRequired] = "A reason is required to suspend a policy.",
        [PolicyErrorCodes.InvalidStatusTransition] = "A policy cannot change from {0} to {1}.",
        [PolicyErrorCodes.AlreadyCancelled] = "The policy is already cancelled.",
        [PolicyErrorCodes.RateChangeOnCancelled] = "The base rate of a cancelled policy cannot be changed.",
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            return false;
        }

        var status = domainException.Kind == DomainErrorKind.Conflict
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;

        var template = Messages.GetValueOrDefault(domainException.Code, domainException.Code);

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = string.Format(template, [.. domainException.Arguments]),
                Extensions = { ["code"] = domainException.Code },
            },
        });
    }
}
