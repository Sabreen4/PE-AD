using System.Linq.Expressions;
using MetaDataService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Shared.Common.Constants;
using Shared.Common.Entities;
using Shared.Common.Extensions;

namespace MetaDataService.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Metadata Service.
/// Manages reference sets, reference terms, and their many-to-many mappings.
/// </summary>
public class MetaDataDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="MetaDataDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">EF Core database context options.</param>
    private readonly ILogger<MetaDataDbContext> _logger;

    public MetaDataDbContext(DbContextOptions<MetaDataDbContext> options, ILogger<MetaDataDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>Gets or sets the reference sets table.</summary>
    public DbSet<RefSet> RefSets { get; set; }

    /// <summary>Gets or sets the reference terms table.</summary>
    public DbSet<RefTerm> RefTerms { get; set; }

    /// <summary>Gets or sets the ref-set-to-ref-term mappings table.</summary>
    public DbSet<SetRefTerm> SetRefTerms { get; set; }

    /// <summary>
    /// Configures entity mappings, the default schema, primary/foreign keys,
    /// global query filters, snake_case naming conventions, and UTC datetime
    /// converters for all entities.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaConstants.METADATA_SCHEMA);

        // ---------------- RefSet ----------------
        modelBuilder.Entity<RefSet>(entity =>
        {
            entity.HasKey(rs => rs.Id);

            entity.Property(rs => rs.Id).ValueGeneratedNever();
            entity.Property(rs => rs.Name).IsRequired().HasMaxLength(150);
            entity.Property(rs => rs.Description).HasMaxLength(500);

            entity.HasIndex(rs => rs.Name).IsUnique();

            entity
                .HasMany(rs => rs.SetRefTerms)
                .WithOne(srt => srt.RefSet)
                .HasForeignKey(srt => srt.RefSetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- RefTerm ----------------
        modelBuilder.Entity<RefTerm>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Id).ValueGeneratedNever();
            entity.Property(rt => rt.Name).IsRequired().HasMaxLength(150);
            entity.Property(rt => rt.Description).HasMaxLength(500);

            entity.HasIndex(rt => rt.Name);

            entity
                .HasMany(rt => rt.SetRefTerms)
                .WithOne(srt => srt.RefTerm)
                .HasForeignKey(srt => srt.RefTermId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- SetRefTerm ----------------
        modelBuilder.Entity<SetRefTerm>(entity =>
        {
            entity.HasKey(srt => srt.Id);

            entity.Property(srt => srt.Id).ValueGeneratedNever();

            entity
                .HasOne(srt => srt.RefSet)
                .WithMany(rs => rs.SetRefTerms)
                .HasForeignKey(srt => srt.RefSetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(srt => srt.RefTerm)
                .WithMany(rt => rt.SetRefTerms)
                .HasForeignKey(srt => srt.RefTermId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasIndex(srt => new { srt.RefSetId, srt.RefTermId })
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