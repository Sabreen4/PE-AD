using System.Linq.Expressions;
using ProgramService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Shared.Common.Constants;
using Shared.Common.Entities;
using Shared.Common.Extensions;

namespace ProgramService.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Program Service.
/// Manages all program-related entities including programs, program weeks,
/// assignments, enrollments, and employee assignments.
/// </summary>
public class ProgramDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="ProgramDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">EF Core database context options.</param>
    private readonly ILogger<ProgramDbContext> _logger;

    public ProgramDbContext(DbContextOptions<ProgramDbContext> options, ILogger<ProgramDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>Gets or sets the training programs table.</summary>
    public DbSet<TrainingPrograms> TrainingPrograms { get; set; }

    /// <summary>Gets or sets the program weeks table.</summary>
    public DbSet<ProgramWeek> ProgramWeeks { get; set; }

    /// <summary>Gets or sets the assignments table.</summary>
    public DbSet<Assignment> Assignments { get; set; }

    /// <summary>Gets or sets the enrollments table.</summary>
    public DbSet<Enrollment> Enrollments { get; set; }

    /// <summary>Gets or sets the employee assignments table.</summary>
    public DbSet<EmployeeAssignment> EmployeeAssignments { get; set; }

    /// <summary>
    /// Configures entity mappings, the default schema, primary/foreign keys,
    /// global query filters, snake_case naming conventions, and UTC datetime
    /// converters for all entities.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaConstants.PROGRAM_SCHEMA);

        // ---------------- TrainingPrograms ----------------
        modelBuilder.Entity<TrainingPrograms>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.StatusId).IsRequired(false);

            entity.HasIndex(p => p.Name);

            entity
                .HasMany(p => p.ProgramWeeks)
                .WithOne(pw => pw.TrainingPrograms)
                .HasForeignKey(pw => pw.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(p => p.Assignments)
                .WithOne(a => a.TrainingPrograms)
                .HasForeignKey(a => a.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(p => p.Enrollments)
                .WithOne(e => e.TrainingPrograms)
                .HasForeignKey(e => e.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- ProgramWeek ----------------
        modelBuilder.Entity<ProgramWeek>(entity =>
        {
            entity.HasKey(pw => pw.Id);

            entity.Property(pw => pw.Title).IsRequired().HasMaxLength(200);

            entity.Property(pw => pw.VideoName).HasMaxLength(255);
            entity.Property(pw => pw.VideoPath).HasMaxLength(500);
            entity.Property(pw => pw.ThumbnailPath).HasMaxLength(500);
            entity.Property(pw => pw.VideoDuration).HasColumnType("time");

            entity
                .HasIndex(pw => new { pw.TrainingProgramId, pw.WeekNumber })
                .IsUnique();

            entity
                .HasOne(pw => pw.TrainingPrograms)
                .WithMany(p => p.ProgramWeeks)
                .HasForeignKey(pw => pw.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(pw => pw.Assignments)
                .WithOne(a => a.ProgramWeek)
                .HasForeignKey(a => a.ProgramWeekId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Assignment ----------------
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.FilePath);
            entity.Property(a => a.MaxMarks);

            entity
                .HasOne(a => a.TrainingPrograms)
                .WithMany(p => p.Assignments)
                .HasForeignKey(a => a.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(a => a.ProgramWeek)
                .WithMany(pw => pw.Assignments)
                .HasForeignKey(a => a.ProgramWeekId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(a => a.EmployeeAssignments)
                .WithOne(ea => ea.Assignment)
                .HasForeignKey(ea => ea.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Enrollment ----------------
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.StatusId).IsRequired(false);

            entity
                .HasIndex(e => new { e.TrainingProgramId, e.EmployeeId })
                .IsUnique();

            entity
                .HasOne(e => e.TrainingPrograms)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(e => e.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(e => e.EmployeeAssignments)
                .WithOne(ea => ea.Enrollment)
                .HasForeignKey(ea => ea.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- EmployeeAssignment ----------------
        modelBuilder.Entity<EmployeeAssignment>(entity =>
        {
            entity.HasKey(ea => ea.Id);

            entity.Property(ea => ea.StatusId).IsRequired(false);
            entity.Property(ea => ea.FileName).HasMaxLength(255);
            entity.Property(ea => ea.FilePath);
            entity.Property(ea => ea.Marks).HasColumnType("decimal(5,2)");
            entity.Property(ea => ea.ReviewerComment).HasMaxLength(1000);

            entity
                .HasOne(ea => ea.Assignment)
                .WithMany(a => a.EmployeeAssignments)
                .HasForeignKey(ea => ea.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(ea => ea.Enrollment)
                .WithMany(e => e.EmployeeAssignments)
                .HasForeignKey(ea => ea.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Global conventions ----------------
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Apply global soft-delete filter to all BaseEntity-derived types
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
