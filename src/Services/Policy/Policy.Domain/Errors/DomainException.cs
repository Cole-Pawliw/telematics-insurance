namespace Policy.Domain.Errors;

/// <summary>
/// Raised when a domain rule is broken. Carries an error code and arguments rather than
/// a message; the API layer turns the code into a readable message.
/// </summary>
public sealed class DomainException : Exception
{
    private DomainException(string code, DomainErrorKind kind, object[] arguments)
        : base(code)
    {
        Code = code;
        Kind = kind;
        Arguments = arguments;
    }

    public string Code { get; }

    public DomainErrorKind Kind { get; }

    public IReadOnlyList<object> Arguments { get; }

    public static DomainException Validation(string code, params object[] arguments) =>
        new(code, DomainErrorKind.Validation, arguments);

    public static DomainException Conflict(string code, params object[] arguments) =>
        new(code, DomainErrorKind.Conflict, arguments);
}
