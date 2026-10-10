namespace IdentityService.Application.Dtos.Users;

/// <summary>
/// Data transfer object for creating a new user.
/// </summary>
public sealed class CreateUserRequestDto
{
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the phone number.</summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>Gets or sets the organization identifier.</summary>
    public long OrganizationId { get; set; }
    /// <summary>Gets or sets the role type (Employee or Evaluator).</summary>
    public string RoleType { get; set; } = string.Empty;
    /// <summary>Gets or sets the gender.</summary>
    public string Gender { get; set; } = string.Empty;
}
