using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions;

/// <summary>
/// Thrown when a request is invalid or malformed.
/// Returns HTTP 400 Bad Request.
/// </summary>
public class BadRequestException : BaseException
{
    public BadRequestException()
        : base("The request is invalid.", StatusCodes.Status400BadRequest) { }

    public BadRequestException(string message)
        : base(message, StatusCodes.Status400BadRequest) { }
}
