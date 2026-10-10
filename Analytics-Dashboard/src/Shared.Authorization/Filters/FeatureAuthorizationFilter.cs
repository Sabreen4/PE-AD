using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.Common.Dtos;
using Shared.InterServiceHelper.Interfaces;

namespace Shared.Authorization.Filters;

public class FeatureAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string[] _featureCodes;
    private readonly IRbacGrpcClient _authorizationService;
    private readonly IUserContextService _userContext;
    private readonly ILogger<FeatureAuthorizationFilter> _logger;

    public FeatureAuthorizationFilter(
        string[] featureCodes,
        IRbacGrpcClient authorizationService,
        IUserContextService userContext,
        ILogger<FeatureAuthorizationFilter> logger
    )
    {
        _featureCodes = featureCodes;
        _authorizationService = authorizationService;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        Guid? userId = _userContext.UserId;

        if (userId is null)
        {
            context.Result = ErrorResult(
                StatusCodes.Status401Unauthorized,
                "Authentication required. Please log in to continue"
            );
            return;
        }

        if (_userContext.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            return;

        string bearerToken = context.HttpContext.Request.Headers.Authorization.ToString();

        foreach (string featureCode in _featureCodes)
        {
            bool isAllowed = await _authorizationService.CheckFeatureAccessAsync(
                userId.Value,
                featureCode,
                bearerToken
            );

            if (isAllowed)
                return;
        }

        context.Result = ErrorResult(
            StatusCodes.Status403Forbidden,
            "You do not have permission to perform this action"
        );
    }

    private static IActionResult ErrorResult(int statusCode, string message) =>
        new ObjectResult(new ErrorResponseDto(message)) { StatusCode = statusCode };
}