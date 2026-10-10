namespace MetaDataService.Application.Dtos;

/// <summary>
/// DTO representing a reference set.
/// </summary>
public sealed record RefSetResponseDto
{
    /// <summary>Gets the unique identifier of the reference set.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the key (lowercased name) of the reference set.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Gets the display name of the reference set.</summary>
    public string Name { get; init; } = string.Empty;
}
