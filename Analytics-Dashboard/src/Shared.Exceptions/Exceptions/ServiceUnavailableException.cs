namespace Shared.Exceptions;

/// <summary>
/// Thrown when an external service is unreachable or returns an unavailable status.
/// Returns HTTP 503 Service Unavailable.
/// </summary>
public sealed class ServiceUnavailableException : BaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
    /// </summary>
    /// <param name="message">A human-readable description of the error.</param>
    public ServiceUnavailableException(string message)
        : base(message, 503)
    {
    }
}
