using System.Linq.Expressions;
using EvaluationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Shared.Common.Constants;
using Shared.Common.Entities;
using Shared.Common.Extensions;

namespace EvaluationService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Evaluation service, configuring entity mappings, query filters, and audit fields.
/// </summary>
public class EvaluationDbContext : DbContext
{
    private readonly ILogger<EvaluationDbContext> _logger;

    public EvaluationDbContext(DbContextOptions<EvaluationDbContext> options, ILogger<EvaluationDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<Attendance> Attendances { get; set; }

    public DbSet<WeeklyEvaluation> WeeklyEvaluations { get; set; }

    public DbSet<Feedback> Feedbacks { get; set; }

    public DbSet<Leaderboard> Leaderboards { get; set; }

    public DbSet<AssessmentEvaluation> AssessmentEvaluations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaConstants.EVALUATION_SCHEMA);

        // ---------------- Attendance ----------------
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.EnrollmentId).IsRequired();
            entity.Property(a => a.ProgramWeekId).IsRequired();
            entity.Property(a => a.AttendanceDate).IsRequired();
            entity.Property(a => a.AttendanceStatusId).IsRequired();

            entity
                .HasIndex(a => new { a.EnrollmentId, a.ProgramWeekId, a.AttendanceDate })
                .IsUnique();
        });

        // ---------------- WeeklyEvaluation ----------------
        modelBuilder.Entity<WeeklyEvaluation>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.Property(w => w.ProgramWeekId).IsRequired();
            entity.Property(w => w.EmployeeAssignmentId).IsRequired();
            entity.Property(w => w.EnrollmentId).IsRequired();
            entity.Property(w => w.AttendanceScore).HasColumnType("decimal(5,2)");
            entity.Property(w => w.WeeklyAverage).HasColumnType("decimal(5,2)");

            entity
                .HasIndex(w => new { w.EnrollmentId, w.ProgramWeekId })
                .IsUnique();
        });

        // ---------------- Feedback ----------------
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.WeeklyEvaluationId).IsRequired();
            entity.Property(f => f.EmployeeId).IsRequired();
            entity.Property(f => f.FeedbackTypeId).IsRequired();
            entity.Property(f => f.FeedbackTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(f => f.FeedbackTypeName).HasMaxLength(200).IsRequired();
            entity.Property(f => f.FeedbackScoreAdjustment).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(f => f.Comment).HasMaxLength(2000);

            entity
                .HasOne(f => f.WeeklyEvaluation)
                .WithMany(w => w.Feedbacks)
                .HasForeignKey(f => f.WeeklyEvaluationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasIndex(f => new { f.EmployeeId, f.WeeklyEvaluationId })
                .IsUnique();
        });

        // ---------------- Leaderboard ----------------
        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.EnrollmentId).IsRequired();
            entity.Property(l => l.OverallScore).HasColumnType("decimal(5,2)");
            entity.Property(l => l.Rank).IsRequired();

            entity
                .HasIndex(l => l.EnrollmentId)
                .IsUnique();
        });

        // ---------------- AssessmentEvaluation ----------------
        modelBuilder.Entity<AssessmentEvaluation>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.EmployeeAssignmentId).IsRequired();
            entity.Property(a => a.Score).HasColumnType("decimal(5,2)");
            entity.Property(a => a.Remarks).HasMaxLength(2000);
            entity.HasIndex(a => a.EmployeeAssignmentId).IsUnique();
        });

        // ---------------- Global conventions ----------------
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                ParameterExpression param = Expression.Parameter(entityType.ClrType, "e");
                LambdaExpression? filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(param, nameof(BaseEntity.IsActive)),
                        Expression.Constant(true)
                    ),
                    param
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            entityType.SetTableName(entityType.GetTableName()!.ConvertToSnakeCase());

            foreach (IMutableProperty property in entityType.GetProperties())
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

            foreach (IMutableKey key in entityType.GetKeys())
                key.SetName(key.GetName()!.ConvertToSnakeCase());

            foreach (IMutableForeignKey fk in entityType.GetForeignKeys())
                fk.SetConstraintName(fk.GetConstraintName()!.ConvertToSnakeCase());

            foreach (IMutableIndex index in entityType.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()!.ConvertToSnakeCase());
        }
    }

    /// <summary>
    /// Applies audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive) to tracked entities before saving.
    /// </summary>
    /// <param name="currentUserId">The current user's identifier.</param>
    public void OnBeforeSaving(Guid currentUserId)
    {
        _logger.LogDebug("Applying audit fields for user {UserId}", currentUserId);
        DateTime utcNow = DateTime.UtcNow;

        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
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
    /// Saves all changes with audit fields applied for the given user.
    /// </summary>
    /// <param name="currentUserId">The current user's identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
