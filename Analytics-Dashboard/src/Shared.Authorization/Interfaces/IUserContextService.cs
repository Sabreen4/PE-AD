namespace Shared.Authorization.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's identity context
/// for the duration of the request.
/// </summary>
public interface IUserContextService
{
    /// <summary>Gets the current user's unique identifier, or <see langword="null"/> if not authenticated.</summary>
    Guid? UserId { get; }
    /// <summary>Gets the list of roles assigned to the current user.</summary>
    IReadOnlyList<string> Roles { get; }
    /// <summary>Gets the organization identifier for the current user.</summary>
    Int64 OrganizationId { get; }

    /// <summary>
    /// Sets the current user's identity context including user ID, roles, and organization.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="roles">The list of roles assigned to the user.</param>
    /// <param name="organizationId">The organization identifier.</param>
    void SetUser(Guid userId, List<string> roles, Int64 organizationId);
}