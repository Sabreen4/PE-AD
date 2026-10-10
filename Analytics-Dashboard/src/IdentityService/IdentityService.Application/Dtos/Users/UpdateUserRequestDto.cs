namespace IdentityService.Application.Dtos.Users;

/// <summary>
/// Data transfer object for updating a user's profile.
/// </summary>
public sealed class UpdateUserRequestDto
{
    /// <summary>Gets or sets the updated first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the updated last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the updated phone number.</summary>
    public string Phone { get; set; } = string.Empty;
}
