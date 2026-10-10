namespace MetaDataService.Application.Dtos;

/// <summary>
/// DTO representing a reference term associated with a reference set.
/// </summary>
public sealed record RefSetTermResponseDto
{
    /// <summary>Gets the unique identifier of the reference term.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the identifier of the parent reference set.</summary>
    public Guid RefSetId { get; init; }

    /// <summary>Gets the key (name) of the reference term.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Gets the display value (description) of the reference term.</summary>
    public string Value { get; init; } = string.Empty;
}
