namespace Shared.Common.Constants;

/// <summary>
/// Constants for custom JWT claim types used across all microservices.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// The claim type used to convey the user's organization identifier.
    /// </summary>
    public const string OrganizationId = "organizationId";
}
