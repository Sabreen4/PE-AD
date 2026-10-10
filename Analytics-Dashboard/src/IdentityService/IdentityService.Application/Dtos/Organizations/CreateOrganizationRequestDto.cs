namespace IdentityService.Application.Dtos.Organizations;

/// <summary>
/// Data transfer object for creating a new organization with an initial admin user.
/// </summary>
public sealed class CreateOrganizationRequestDto
{
    /// <summary>Gets or sets the organization name.</summary>
    public string OrganizationName { get; set; } = string.Empty;
    /// <summary>Gets or sets the organization email.</summary>
    public string OrganizationEmail { get; set; } = string.Empty;
    /// <summary>Gets or sets the organization phone number.</summary>
    public string OrganizationPhone { get; set; } = string.Empty;
    /// <summary>Gets or sets the city where the organization is located.</summary>
    public string City { get; set; } = string.Empty;
    /// <summary>Gets or sets the admin user's first name.</summary>
    public string AdminFirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the admin user's last name.</summary>
    public string AdminLastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the admin user's email address.</summary>
    public string AdminEmail { get; set; } = string.Empty;
    /// <summary>Gets or sets the admin user's phone number.</summary>
    public string AdminPhone { get; set; } = string.Empty;
    /// <summary>Gets or sets the admin user's gender.</summary>
    public string AdminGender { get; set; } = string.Empty;
}

