using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;

namespace Shared.Authorization.Services;

/// <summary>
/// Stores the current user's identity context (UserId, Roles, OrganizationId)
/// for the duration of the request. Set by <see cref="UserContextMiddleware"/>.
/// </summary>
public class UserContextService : IUserContextService
{
    private readonly ILogger<UserContextService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContextService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public UserContextService(ILogger<UserContextService> logger)
    {
        _logger = logger;
    }

    /// <summary>Gets the current user's unique identifier, or <see langword="null"/> if not authenticated.</summary>
    public Guid? UserId { get; private set; }
    /// <summary>Gets the list of roles assigned to the current user.</summary>
    public IReadOnlyList<string> Roles { get; private set; } = new List<string>();
    /// <summary>Gets the organization identifier for the current user.</summary>
    public Int64 OrganizationId { get; private set; }

    /// <summary>
    /// Sets the current user's identity context for the duration of the request.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="roles">The list of roles assigned to the user.</param>
    /// <param name="organizationId">The organization identifier.</param>
    public void SetUser(Guid userId, List<string> roles, Int64 organizationId)
    {
        _logger.LogInformation("Setting user context: UserId={UserId}, Roles={Roles}, OrganizationId={OrganizationId}",
            userId, string.Join(", ", roles), organizationId);
        UserId = userId;
        Roles = roles;
        OrganizationId = organizationId;
    }
}
