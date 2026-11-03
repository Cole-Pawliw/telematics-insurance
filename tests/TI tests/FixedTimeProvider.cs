namespace TelematicsInsurance.Tests;

/// <summary>
/// A <see cref="TimeProvider"/> that always returns the same instant.
/// </summary>
internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
