namespace Shared.Exceptions;

/// <summary>
/// Base class for all custom exceptions across all microservices.
/// Each derived exception carries its own HTTP status code so the
/// exception handler never hard-codes status values itself.
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// The HTTP status code to return when this exception is caught.
    /// </summary>
    public int StatusCode { get; }

    protected BaseException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
