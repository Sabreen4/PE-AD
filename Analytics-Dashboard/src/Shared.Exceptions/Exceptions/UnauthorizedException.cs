using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions;

/// <summary>
/// Thrown when a user is not authenticated.
/// Returns HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : BaseException
{
    public UnauthorizedException()
        : base("You are not authorized to perform this action.", StatusCodes.Status401Unauthorized)
    { }

    public UnauthorizedException(string message)
        : base(message, StatusCodes.Status401Unauthorized) { }
}
