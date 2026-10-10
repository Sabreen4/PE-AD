namespace IdentityService.Application.Dtos.Users;

/// <summary>
/// Data transfer object for a user response.
/// </summary>
public sealed class UserResponseDto
{
    /// <summary>Gets or sets the user identifier.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the phone number.</summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>Gets or sets the role name.</summary>
    public string Role { get; set; } = string.Empty;
    /// <summary>Gets or sets the organization identifier.</summary>
    public long OrganizationId { get; set; }
}
