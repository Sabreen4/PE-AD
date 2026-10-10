using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current state of a resource.
/// Returns HTTP 409 Conflict.
/// </summary>
public class ConflictException : BaseException
{
    public ConflictException()
        : base(
            "A conflict occurred with the current state of the resource.",
            StatusCodes.Status409Conflict
        ) { }

    public ConflictException(string message)
        : base(message, StatusCodes.Status409Conflict) { }

    public ConflictException(string name, object key)
        : base($"{name} with '{key}' already exists.", StatusCodes.Status409Conflict) { }
}
