using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.Authorization.Services;
using Shared.Common.Constants;

namespace Shared.Authorization.Middleware;

/// <summary>
/// Middleware that extracts the authenticated user's identity from the JWT token
/// and populates <see cref="IUserContextService"/> for the current request.
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserContextMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContextMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the request pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, extracting user context claims from the current HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("Processing authenticated request for user {UserName}", context.User.Identity.Name);
            IUserContextService userService = context.RequestServices.GetRequiredService<IUserContextService>();

            if (userService is UserContextService impl)
            {
                Claim? userIdClaim =
                    context.User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);

                List<string> roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                Claim? orgClaim = context.User.FindFirst(CustomClaimTypes.OrganizationId);

                if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Int64 organizationId = orgClaim is not null && Int64.TryParse(orgClaim.Value, out Int64 parsedOrg)
                        ? parsedOrg
                        : 0;

                    _logger.LogInformation("Setting user context for UserId: {UserId}, Roles: {Roles}, OrgId: {OrgId}", userId, roles, organizationId);
                    impl.SetUser(userId, roles, organizationId);
                }
                else
                {
                    _logger.LogWarning("UserId claim not found or invalid for authenticated request");
                }
            }
        }
        else
        {
            _logger.LogDebug("Processing unauthenticated request");
        }

        await _next(context);
    }
}