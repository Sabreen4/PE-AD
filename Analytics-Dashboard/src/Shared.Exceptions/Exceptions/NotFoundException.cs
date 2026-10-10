using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// Returns HTTP 404 Not Found.
/// </summary>
public class NotFoundException : BaseException
{
    public NotFoundException()
        : base("The requested resource was not found.", StatusCodes.Status404NotFound) { }

    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }

    public NotFoundException(string name, object key)
        : base($"{name} with id '{key}' was not found.", StatusCodes.Status404NotFound) { }
}
