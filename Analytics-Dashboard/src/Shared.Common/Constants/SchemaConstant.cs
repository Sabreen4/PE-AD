namespace Shared.Common.Constants;

/// <summary>
/// Centralised database schema name constants shared across all microservices.
/// </summary>
/// <remarks>
/// Using a single constant prevents typos and makes schema-wide renames
/// a one-line change instead of a grep-and-replace across every
/// <c>DbContext</c> and Fluent API configuration.
/// </remarks>
public static class SchemaConstants
{
    /// <summary>
    /// The PostgreSQL schema name used by every microservice in this solution.
    /// </summary>
    /// <remarks>
    /// Apply this via EF Core's <c>modelBuilder.HasDefaultSchema(...)</c>
    /// in each <c>DbContext.OnModelCreating</c> override so all tables
    /// land in the same schema by default.
    /// </remarks>
    /// <value><c>"smart_recruitment_system"</c></value>
    public const string IDENTITY_SCHEMA = "pead_identity_service";
    /// <summary>The PostgreSQL schema name for the Evaluation microservice.</summary>
    public const string EVALUATION_SCHEMA = "pead_evaluation_service";
    /// <summary>The PostgreSQL schema name for the Program microservice.</summary>
    public const string PROGRAM_SCHEMA = "pead_program_service";
    /// <summary>The PostgreSQL schema name for the Notification microservice.</summary>
    public const string NOTIFICATION_SCHEMA = "pead_notification_service";
    /// <summary>The PostgreSQL schema name for the MetaData microservice.</summary>
    public const string METADATA_SCHEMA = "pead_metadata_service";
}
