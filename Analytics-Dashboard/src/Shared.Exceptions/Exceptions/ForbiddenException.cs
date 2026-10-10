using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions;

/// <summary>
/// Thrown when a user lacks permission to perform an action.
/// Returns HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : BaseException
{
    public ForbiddenException()
        : base("You do not have permission to perform this action.", StatusCodes.Status403Forbidden)
    { }

    public ForbiddenException(string message)
        : base(message, StatusCodes.Status403Forbidden) { }
}
