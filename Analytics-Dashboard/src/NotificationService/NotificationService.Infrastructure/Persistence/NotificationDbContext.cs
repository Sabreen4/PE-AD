using System.Linq.Expressions;
using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Shared.Common.Constants;
using Shared.Common.Entities;
using Shared.Common.Extensions;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Notification Service.
/// Manages notifications and their per-user delivery/read state.
/// </summary>
public class NotificationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="NotificationDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">EF Core database context options.</param>
    private readonly ILogger<NotificationDbContext> _logger;

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options, ILogger<NotificationDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>Gets or sets the notifications table.</summary>
    public DbSet<Notification> Notifications { get; set; }

    /// <summary>Gets or sets the user-notification mappings table.</summary>
    public DbSet<UserNotification> UserNotifications { get; set; }

    /// <summary>
    /// Configures entity mappings, the default schema, primary/foreign keys,
    /// global query filters, snake_case naming conventions, and UTC datetime
    /// converters for all entities.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaConstants.NOTIFICATION_SCHEMA);

        // ---------------- Notification ----------------
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.Property(n => n.NotificationTypeId).IsRequired();
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired();

            entity
                .HasMany(n => n.UserNotifications)
                .WithOne(un => un.Notification)
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- UserNotification ----------------
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(un => un.Id);

            entity.Property(un => un.UserId).IsRequired();
            entity.Property(un => un.NotificationId).IsRequired();
            entity.Property(un => un.IsRead).IsRequired();
            entity.Property(un => un.ReadAt);

            entity
                .HasOne(un => un.Notification)
                .WithMany(n => n.UserNotifications)
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasIndex(un => new { un.UserId, un.NotificationId })
                .IsUnique();
        });

        // ---------------- Global conventions ----------------
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                ParameterExpression param = Expression.Parameter(entityType.ClrType, "e");
                LambdaExpression filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(param, nameof(BaseEntity.IsActive)),
                        Expression.Constant(true)
                    ),
                    param
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            entityType.SetTableName(entityType.GetTableName()!.ConvertToSnakeCase());

            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ConvertToSnakeCase());

                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTime, DateTime>(
                            v =>
                                v.Kind == DateTimeKind.Unspecified
                                    ? DateTime.SpecifyKind(v, DateTimeKind.Utc)
                                    : v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        )
                    );
                }
            }

            foreach (var key in entityType.GetKeys())
                key.SetName(key.GetName()!.ConvertToSnakeCase());

            foreach (var fk in entityType.GetForeignKeys())
                fk.SetConstraintName(fk.GetConstraintName()!.ConvertToSnakeCase());

            foreach (var index in entityType.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()!.ConvertToSnakeCase());
        }
    }

    /// <summary>
    /// Applies audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
    /// to all tracked <see cref="BaseEntity"/> entries before saving.
    /// </summary>
    /// <param name="currentUserId">The ID of the user performing the operation.</param>
    public void OnBeforeSaving(Guid currentUserId)
    {
        _logger.LogDebug("Applying audit fields for user {UserId}", currentUserId);
        DateTime utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.UpdatedBy = currentUserId;
                    if (!entry.Property(nameof(BaseEntity.IsActive)).IsModified)
                        entry.Entity.IsActive = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Entity.UpdatedBy = currentUserId;
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                    break;
            }
        }
    }

    /// <summary>
    /// Applies audit information and persists all pending changes to the database.
    /// </summary>
    /// <param name="currentUserId">The ID of the user performing the operation, used for audit fields.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Saving changes for user {UserId}", currentUserId);
        OnBeforeSaving(currentUserId);
        int result = await base.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Saved {Count} changes for user {UserId}", result, currentUserId);
        return result;
    }
}