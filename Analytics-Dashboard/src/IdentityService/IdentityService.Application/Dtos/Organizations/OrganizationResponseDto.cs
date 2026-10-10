namespace IdentityService.Application.Dtos.Organizations;

/// <summary>
/// Data transfer object for an organization response.
/// </summary>
public sealed class OrganizationResponseDto
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public long Id { get; set; }
    /// <summary>Gets or sets the organization name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the organization email.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the organization phone number.</summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>Gets or sets the city.</summary>
    public string City { get; set; } = string.Empty;
}
