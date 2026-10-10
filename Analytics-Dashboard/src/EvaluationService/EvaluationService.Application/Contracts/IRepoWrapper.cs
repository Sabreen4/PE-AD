using EvaluationService.Application.Contracts.IRepository;

namespace EvaluationService.Application.Contracts;

/// <summary>
/// Abstraction over all evaluation repositories providing a unit-of-work pattern.
/// </summary>
public interface IRepoWrapper
{
    /// <summary>
    /// Gets the attendance repository.
    /// </summary>
    IAttendanceRepository Attendance { get; }
    /// <summary>
    /// Gets the weekly evaluation repository.
    /// </summary>
    IWeeklyEvaluationRepository WeeklyEvaluation { get; }
    /// <summary>
    /// Gets the feedback repository.
    /// </summary>
    IFeedbackRepository Feedback { get; }
    /// <summary>
    /// Gets the leaderboard repository.
    /// </summary>
    ILeaderboardRepository Leaderboard { get; }
    /// <summary>
    /// Gets the assessment evaluation repository.
    /// </summary>
    IAssessmentEvaluationRepository AssessmentEvaluation { get; }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task<Int32> SaveAsync();
}
