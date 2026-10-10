namespace Shared.Common.Dtos;

/// <summary>
/// Standard JSON shape returned for failed requests across all microservices.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>Indicates whether the operation completed successfully. Always <see langword="false"/> for error responses.</summary>
    public bool Success { get; set; } = false;

    /// <summary>A human-readable error description.</summary>
    public string Message { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseDto"/> class with the specified error message.
    /// </summary>
    /// <param name="message">A human-readable error description.</param>
    public ErrorResponseDto(string message)
    {
        Message = message;
    }
}
