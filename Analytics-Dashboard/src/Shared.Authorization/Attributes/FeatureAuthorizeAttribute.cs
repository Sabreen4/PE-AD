using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Filters;

namespace Shared.Authorization.Attributes;

public class FeatureAuthorizeAttribute : TypeFilterAttribute
{
    public FeatureAuthorizeAttribute(string featureCode)
        : base(typeof(FeatureAuthorizationFilter))
    {
        Arguments = new object[] { new[] { featureCode } };
    }

    public FeatureAuthorizeAttribute(params string[] featureCodes)
        : base(typeof(FeatureAuthorizationFilter))
    {
        Arguments = new object[] { featureCodes };
    }
}