namespace Shared.Exceptions;

/// <summary>
/// Thrown when an external service call exceeds the configured timeout.
/// Returns HTTP 504 Gateway Timeout.
/// </summary>
public sealed class ServiceTimeoutException : BaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTimeoutException"/> class.
    /// </summary>
    /// <param name="message">A human-readable description of the error.</param>
    public ServiceTimeoutException(string message)
        : base(message, 504)
    {
    }
}
