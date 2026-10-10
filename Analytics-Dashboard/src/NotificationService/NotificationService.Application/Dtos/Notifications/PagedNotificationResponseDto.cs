namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for a paginated list of notifications.
/// </summary>
public sealed class PagedNotificationResponseDto
{
    /// <summary>Gets or sets the list of notification items on the current page.</summary>
    public List<NotificationResponseDto> Items { get; set; } = new();

    /// <summary>Gets or sets the current page number.</summary>
    public int PageNumber { get; set; }

    /// <summary>Gets or sets the number of items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Gets or sets the total number of records across all pages.</summary>
    public int TotalRecords { get; set; }

    /// <summary>Gets or sets the total number of pages.</summary>
    public int TotalPages { get; set; }

    /// <summary>Gets or sets a value indicating whether a previous page exists.</summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>Gets or sets a value indicating whether a next page exists.</summary>
    public bool HasNextPage { get; set; }
}