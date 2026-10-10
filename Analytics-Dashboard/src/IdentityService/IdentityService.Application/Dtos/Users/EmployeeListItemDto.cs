namespace IdentityService.Application.Dtos.Users;

/// <summary>
/// Data transfer object for an employee list item.
/// </summary>
public sealed record EmployeeListItemDto
{
    /// <summary>Gets or sets the employee identifier.</summary>
    public Guid Id { get; init; }
    /// <summary>Gets or sets the employee's full name.</summary>
    public string FullName { get; init; } = string.Empty;
    /// <summary>Gets or sets the employee's email address.</summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>Gets or sets whether the employee is active.</summary>
    public bool IsActive { get; init; }
    /// <summary>Gets or sets the organization identifier.</summary>
    public long OrganizationId { get; init; }
}
