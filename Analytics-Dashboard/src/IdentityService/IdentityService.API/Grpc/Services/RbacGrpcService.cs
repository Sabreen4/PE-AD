using Grpc.Core;
using IdentityService.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.InterServiceHelper.Grpc.Rbac;

namespace IdentityService.API.Grpc.Services;

/// <summary>
/// gRPC service handling role-based access control (RBAC) checks
/// by verifying user role-feature assignments.
/// </summary>
public sealed class RbacGrpcService : RbacService.RbacServiceBase
{
    private readonly IRepoWrapper _repo;
    private readonly ILogger<RbacGrpcService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RbacGrpcService"/> class.
    /// </summary>
    public RbacGrpcService(IRepoWrapper repo, ILogger<RbacGrpcService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Checks whether a user has access to a specific feature based on their role-feature assignments.
    /// </summary>
    /// <returns>A response indicating whether access is allowed.</returns>
    public override async Task<CheckFeatureAccessResponse> CheckFeatureAccess(
        CheckFeatureAccessRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("RBAC check request: UserId={UserId}, FeatureCode={FeatureCode}", request.UserId, request.FeatureCode);

        try
        {
            Guid userId = Guid.Parse(request.UserId);

            Domain.Entities.User? user = await _repo.User
                .FindByCondition(u => u.Id == userId && u.IsActive)
                .Include(u => u.Role)
                .ThenInclude(r => r.RoleFeatures)
                .ThenInclude(rf => rf.Feature)
                .FirstOrDefaultAsync();

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found or not active for RBAC check", userId);
                return new CheckFeatureAccessResponse { IsAllowed = false };
            }

            _logger.LogInformation("RBAC DEBUG: UserId={UserId}, RoleId={RoleId}, RoleName={RoleName}",
                userId, user.Role?.Id, user.Role?.Name);

            if (user.Role?.RoleFeatures is null)
            {
                _logger.LogWarning("RBAC DEBUG: RoleFeatures collection is null for RoleId={RoleId}", user.Role?.Id);
                return new CheckFeatureAccessResponse { IsAllowed = false };
            }

            _logger.LogInformation("RBAC DEBUG: RoleFeatures count loaded={Count}", user.Role.RoleFeatures.Count);

            foreach (var rf in user.Role.RoleFeatures)
            {
                _logger.LogInformation("RBAC DEBUG: RoleFeature FeatureId={FeatureId}, FeatureKey={FeatureKey}, IsActive={IsActive}",
                    rf.Feature?.Id, rf.Feature?.Key, rf.IsActive);
            }

            bool isAllowed = user.Role.RoleFeatures
                .Any(rf => rf.Feature.Key == request.FeatureCode);

            _logger.LogInformation("RBAC check result for user {UserId}, feature {FeatureCode}: {IsAllowed}",
                userId, request.FeatureCode, isAllowed);

            return new CheckFeatureAccessResponse { IsAllowed = isAllowed };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RBAC check failed for user {UserId} feature {FeatureCode}",
                request.UserId, request.FeatureCode);
            return new CheckFeatureAccessResponse { IsAllowed = false };
        }
    }
}
