using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace NotificationService.Infrastructure.Services;

/// <summary>
/// Resolves notification type identifiers by looking up string keys in the Metadata Service's refset.
/// </summary>
public sealed class MetadataServiceNotificationTypeResolver : INotificationTypeResolver
{
    private static readonly String RefSetKey = "NotificationType";

    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<MetadataServiceNotificationTypeResolver> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataServiceNotificationTypeResolver"/> class.
    /// </summary>
    /// <param name="metaDataClient">Client for querying the Metadata Service.</param>
    /// <param name="logger">Logger instance.</param>
    public MetadataServiceNotificationTypeResolver(
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<MetadataServiceNotificationTypeResolver> logger)
    {
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Resolves a notification type key to its corresponding GUID identifier via the Metadata Service.
    /// </summary>
    /// <param name="key">The notification type string key (e.g. "Email", "IN_APP").</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The resolved GUID for the notification type.</returns>
    public async Task<Guid> ResolveAsync(String key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resolving notification type {Type} from Metadata Service", key);

        RefTermDto? term = await _metaDataClient.GetByKeyAsync(RefSetKey, key, cancellationToken);

        if (term is null)
        {
            _logger.LogError("Notification channel '{Key}' is not configured in Metadata Service RefSet '{RefSet}'", key, RefSetKey);
            throw new InvalidOperationException($"Notification channel '{key}' is not configured in Metadata Service RefSet '{RefSetKey}'.");
        }

        Guid refTermId = Guid.Parse(term.Id);
        _logger.LogInformation("Resolved notification type {Type} with Id {Id}", key, refTermId);
        return refTermId;
    }
}
