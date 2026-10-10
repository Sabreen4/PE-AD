namespace MetaDataService.Application.Dtos;

/// <summary>
/// DTO representing a file type option.
/// </summary>
public sealed record FileTypeResponseDto
{
    /// <summary>Gets the key identifier of the file type.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Gets the display name of the file type.</summary>
    public string DisplayName { get; init; } = string.Empty;
}
