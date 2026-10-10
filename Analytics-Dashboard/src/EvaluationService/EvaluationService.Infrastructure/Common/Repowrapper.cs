using EvaluationService.Application.Contracts;
using EvaluationService.Application.Contracts.IRepository;
using EvaluationService.Infrastructure.Persistence;
using EvaluationService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;

namespace EvaluationService.Infrastructure.Common;

/// <summary>
/// Wraps all evaluation repositories into a single unit of work for convenient access and coordinated saving.
/// </summary>
public sealed class RepoWrapper : IRepoWrapper
{
    private readonly EvaluationDbContext _context;
    private readonly IUserContextService _userContext;
    private readonly ILogger<RepoWrapper> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private IAttendanceRepository? _attendance;
    private IWeeklyEvaluationRepository? _weeklyEvaluation;
    private IFeedbackRepository? _feedback;
    private ILeaderboardRepository? _leaderboard;
    private IAssessmentEvaluationRepository? _assessmentEvaluation;

    public RepoWrapper(EvaluationDbContext context, IUserContextService userContext, ILogger<RepoWrapper> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Gets the attendance repository, lazily initialized.
    /// </summary>
    public IAttendanceRepository Attendance
    {
        get { return _attendance ??= new AttendanceRepository(_context, _loggerFactory.CreateLogger<AttendanceRepository>()); }
    }

    /// <summary>
    /// Gets the weekly evaluation repository, lazily initialized.
    /// </summary>
    public IWeeklyEvaluationRepository WeeklyEvaluation
    {
        get { return _weeklyEvaluation ??= new WeeklyEvaluationRepository(_context, _loggerFactory.CreateLogger<WeeklyEvaluationRepository>()); }
    }

    /// <summary>
    /// Gets the feedback repository, lazily initialized.
    /// </summary>
    public IFeedbackRepository Feedback
    {
        get { return _feedback ??= new FeedbackRepository(_context, _loggerFactory.CreateLogger<FeedbackRepository>()); }
    }

    /// <summary>
    /// Gets the leaderboard repository, lazily initialized.
    /// </summary>
    public ILeaderboardRepository Leaderboard
    {
        get { return _leaderboard ??= new LeaderboardRepository(_context, _loggerFactory.CreateLogger<LeaderboardRepository>()); }
    }

    /// <summary>
    /// Gets the assessment evaluation repository, lazily initialized.
    /// </summary>
    public IAssessmentEvaluationRepository AssessmentEvaluation
    {
        get { return _assessmentEvaluation ??= new AssessmentEvaluationRepository(_context, _loggerFactory.CreateLogger<AssessmentEvaluationRepository>()); }
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveAsync()
    {
        _logger.LogInformation("Saving changes to database");
        Guid currentUserId = _userContext.UserId ?? Guid.Empty;
        int result = await _context.SaveAsync(currentUserId);
        _logger.LogInformation("Saved {Count} changes to database", result);
        return result;
    }
}
