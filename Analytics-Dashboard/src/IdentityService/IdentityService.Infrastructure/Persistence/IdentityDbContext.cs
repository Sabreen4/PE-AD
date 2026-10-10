using System.Linq.Expressions;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Shared.Common.Constants;
using Shared.Common.Entities;
using Shared.Common.Extensions;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Identity Service.
/// Manages organizations, users, roles, features, role-feature mappings,
/// refresh tokens, and forgot-password requests.
/// </summary>
public class IdentityDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">EF Core database context options.</param>
    private readonly ILogger<IdentityDbContext> _logger;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ILogger<IdentityDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>Gets or sets the organizations table.</summary>
    public DbSet<Organization> Organizations { get; set; }

    /// <summary>Gets or sets the users table.</summary>
    public DbSet<User> Users { get; set; }

    /// <summary>Gets or sets the roles table.</summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>Gets or sets the features table.</summary>
    public DbSet<Feature> Features { get; set; }

    /// <summary>Gets or sets the role-feature mappings table.</summary>
    public DbSet<RoleFeature> RoleFeatures { get; set; }

    /// <summary>Gets or sets the refresh tokens table.</summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>Gets or sets the forgot-password requests table.</summary>
    public DbSet<VerifyOtp> VerifyOtps { get; set; }

    /// <summary>
    /// Configures entity mappings, the default schema, primary/foreign keys,
    /// global query filters, snake_case naming conventions, and UTC datetime
    /// converters for all entities.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaConstants.IDENTITY_SCHEMA);

        // ---------------- Organization ----------------
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Name).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Email).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Phone).HasMaxLength(20);
            entity.Property(o => o.City).HasMaxLength(500);

            entity
                .HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Role ----------------
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);

            entity.HasIndex(r => r.Name).IsUnique();

            entity
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(r => r.RoleFeatures)
                .WithOne(rf => rf.Role)
                .HasForeignKey(rf => rf.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Feature ----------------
        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Key).IsRequired().HasMaxLength(150);
           

            entity.HasIndex(f => f.Key).IsUnique();

            entity
                .HasMany(f => f.RoleFeatures)
                .WithOne(rf => rf.Feature)
                .HasForeignKey(rf => rf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- RoleFeature ----------------
        modelBuilder.Entity<RoleFeature>(entity =>
        {
            entity.HasKey(rf => rf.Id);

            entity
                .HasOne(rf => rf.Role)
                .WithMany(r => r.RoleFeatures)
                .HasForeignKey(rf => rf.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(rf => rf.Feature)
                .WithMany(f => f.RoleFeatures)
                .HasForeignKey(rf => rf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasIndex(rf => new { rf.RoleId, rf.FeatureId })
                .IsUnique();
        });

        // ---------------- User ----------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.Property(u => u.GenderId).IsRequired(false);
           


            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Phone);

            entity
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(u => u.VerifyOtps)
                .WithOne(fp => fp.User)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- RefreshToken ----------------
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token).IsRequired();
            entity.Property(rt => rt.ExpiryDate).IsRequired();
            entity.Property(rt => rt.IsRevoked).IsRequired();

            entity.HasIndex(rt => rt.Token).IsUnique();

            entity
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- ForgotPassword ----------------
        modelBuilder.Entity<VerifyOtp>(entity =>
        {
            entity.HasKey(fp => fp.Id);

            entity.Property(fp => fp.Otp).IsRequired().HasMaxLength(10);
            entity.Property(fp => fp.ExpiryDate).IsRequired();
            entity.Property(fp => fp.IsVerified).IsRequired();

            entity
                .HasOne(fp => fp.User)
                .WithMany(u => u.VerifyOtps)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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