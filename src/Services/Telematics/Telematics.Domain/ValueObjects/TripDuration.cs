using Telematics.Domain.Errors;

namespace Telematics.Domain.ValueObjects;

/// <summary>
/// When a trip started and ended, always stored in UTC.
/// </summary>
public sealed record TripDuration
{
    public TripDuration(DateTime startTimeUtc, DateTime endTimeUtc)
    {
        var start = ToUtc(startTimeUtc);
        var end = ToUtc(endTimeUtc);

        if (end <= start)
        {
            throw DomainException.Validation(TripErrorCodes.EndNotAfterStart);
        }

        StartTimeUtc = start;
        EndTimeUtc = end;
    }

    public DateTime StartTimeUtc { get; }

    public DateTime EndTimeUtc { get; }

    public TimeSpan Elapsed => EndTimeUtc - StartTimeUtc;

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value,
    };
}
