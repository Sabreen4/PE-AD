namespace IdentityService.Application.Dtos.Common;

/// <summary>
/// Generic data transfer object containing an identifier and a result message.
/// </summary>
public sealed class IdResponseDto
{
    /// <summary>Gets or sets the identifier value.</summary>
    public object Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the result message.</summary>
    public string Message { get; set; } = string.Empty;
}

