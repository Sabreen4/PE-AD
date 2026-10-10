using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions;

/// <summary>
/// Thrown when an operation is canceled (e.g. client disconnected or timeout).
/// Returns HTTP 503 Service Unavailable.
/// </summary>
public class OperationCanceledException : BaseException
{
    public OperationCanceledException()
        : base(
            "The operation was canceled due to a timeout or client disconnect.",
            StatusCodes.Status503ServiceUnavailable
        ) { }

    public OperationCanceledException(string message)
        : base(message, StatusCodes.Status503ServiceUnavailable) { }
}
