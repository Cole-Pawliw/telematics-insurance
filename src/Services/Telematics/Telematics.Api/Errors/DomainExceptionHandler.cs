using Microsoft.AspNetCore.Diagnostics;

using Telematics.Domain.Errors;

namespace Telematics.Api.Errors;

/// <summary>
/// Turns broken domain rules into 400/409 problem responses with a readable message.
/// </summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        [TripErrorCodes.PolicyNumberRequired] = "Policy number is required.",
        [TripErrorCodes.PolicyNumberTooLong] = "Policy number cannot be longer than {0} characters.",
        [TripErrorCodes.EndNotAfterStart] = "Trip end time must be after its start time.",
        [TripErrorCodes.MilesDrivenNegative] = "Miles driven cannot be negative.",
        [TripErrorCodes.HardBrakeCountNegative] = "Hard brake count cannot be negative.",
        [TripErrorCodes.RapidAccelerationCountNegative] = "Rapid acceleration count cannot be negative.",
        [TripErrorCodes.MaxSpeedNegative] = "Max speed cannot be negative.",
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
