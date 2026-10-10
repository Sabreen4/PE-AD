using System;

namespace Shared.Common.Entities;

/// <summary>
/// Base class for all domain entities across every microservice.
/// Provides the standard audit and soft-delete columns that every
/// table in the system must track.
/// </summary>
/// <remarks>
/// <para>
/// Every entity that maps to a database table should inherit from this class
/// so EF Core picks up the audit fields automatically via the shared
/// <c>BaseRepository</c> save hooks.
/// </para>
/// <para>
/// Soft deletes are handled through <see cref="IsActive"/>. Queries should
/// always filter on <c>IsActive == true</c> unless an admin screen
/// explicitly needs to surface deactivated records.
/// </para>
/// </remarks>
public class BaseEntity
{
    /// <summary>
    /// Gets or sets the <see cref="Guid"/> of the user who created this record.
    /// </summary>
    /// <remarks>
    /// Set automatically from the authenticated user's identity claim
    /// in the repository save hook before the first <c>INSERT</c>.
    /// </remarks>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Guid"/> of the user who last modified this record.
    /// </summary>
    /// <remarks>
    /// Updated automatically from the authenticated user's identity claim
    /// in the repository save hook on every <c>UPDATE</c>.
    /// </remarks>
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this record was first created.
    /// </summary>
    /// <remarks>
    /// Set once by the repository save hook before the initial <c>INSERT</c>
    /// and never changed thereafter.
    /// </remarks>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this record was last updated.
    /// </summary>
    /// <remarks>
    /// Refreshed by the repository save hook on every <c>UPDATE</c>.
    /// Equals <see cref="CreatedAt"/> immediately after the first insert.
    /// </remarks>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this record is logically active.
    /// </summary>
    /// <remarks>
    /// <c>true</c> means the record is visible and usable; <c>false</c> is a
    /// soft-delete — the row remains in the database but is excluded from
    /// all normal queries. Defaults to <c>true</c> on creation.
    /// </remarks>
    public bool IsActive { get; set; }
}
