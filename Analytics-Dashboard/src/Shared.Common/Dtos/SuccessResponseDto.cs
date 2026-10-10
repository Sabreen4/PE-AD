namespace Shared.Common.Dtos;

/// <summary>
/// Standard JSON shape returned for successful requests across all microservices.
/// </summary>
public class SuccessResponseDto<T>
{
    /// <summary>Indicates whether the operation completed successfully.</summary>
    public bool Success { get; set; } = true;

    /// <summary>A human-readable message describing the result.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>The response payload, if any.</summary>
    public T? Data { get; set; }
}

/// <summary>Non-generic variant for responses with no payload.</summary>
public class SuccessResponseDto : SuccessResponseDto<object?>
{
}