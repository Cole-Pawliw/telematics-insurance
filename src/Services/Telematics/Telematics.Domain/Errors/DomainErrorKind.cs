namespace Telematics.Domain.Errors;

/// <summary>
/// Classifies a broken domain rule so callers can report it appropriately.
/// </summary>
public enum DomainErrorKind
{
    /// <summary>The input breaks a rule.</summary>
    Validation,

    /// <summary>The request conflicts with the current state.</summary>
    Conflict,
}
