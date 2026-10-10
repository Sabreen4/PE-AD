using EvaluationService.Domain.Entities;

namespace EvaluationService.Application.Contracts;

/// <summary>
/// Defines evaluation service operations for attendance, weekly evaluations, feedback, leaderboard, and assessments.
/// </summary>
public interface IEvaluationService
{
    /// <summary>
    /// Retrieves all attendance records for a given enrollment.
    /// </summary>
    Task<List<Attendance>> GetAttendancesByEnrollmentAsync(long enrollmentId);
    /// <summary>
    /// Retrieves attendance records for a given enrollment and program week.
    /// </summary>
    Task<List<Attendance>> GetAttendancesByEnrollmentAndWeekAsync(long enrollmentId, long programWeekId);
    /// <summary>
    /// Checks whether an attendance record exists for the given enrollment, program week, and date.
    /// </summary>
    Task<bool> AttendanceExistsForDateAsync(long enrollmentId, long programWeekId, DateOnly attendanceDate);
    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    Task CreateAttendanceAsync(Attendance entity);
    /// <summary>
    /// Updates an existing attendance record.
    /// </summary>
    Task UpdateAttendanceAsync(Attendance entity);
    /// <summary>
    /// Deletes an attendance record.
    /// </summary>
    Task DeleteAttendanceAsync(Attendance entity);
    /// <summary>
    /// Retrieves an attendance record by its identifier.
    /// </summary>
    Task<Attendance?> GetAttendanceByIdAsync(long attendanceId);

    /// <summary>
    /// Retrieves all weekly evaluations.
    /// </summary>
    Task<List<WeeklyEvaluation>> GetAllWeeklyEvaluationsAsync();
    /// <summary>
    /// Retrieves a weekly evaluation by its identifier.
    /// </summary>
    Task<WeeklyEvaluation?> GetWeeklyEvaluationByIdAsync(long weeklyEvaluationId);
    /// <summary>
    /// Checks whether a weekly evaluation exists for the given enrollment and program week.
    /// </summary>
    Task<bool> WeeklyEvaluationExistsAsync(long enrollmentId, long programWeekId);
    /// <summary>
    /// Creates a new weekly evaluation.
    /// </summary>
    Task CreateWeeklyEvaluationAsync(WeeklyEvaluation entity);
    /// <summary>
    /// Updates an existing weekly evaluation.
    /// </summary>
    Task UpdateWeeklyEvaluationAsync(WeeklyEvaluation entity);
    /// <summary>
    /// Retrieves weekly evaluations for a given enrollment.
    /// </summary>
    Task<List<WeeklyEvaluation>> GetWeeklyEvaluationsByEnrollmentAsync(long enrollmentId);
    /// <summary>
    /// Retrieves the weekly evaluation for a given enrollment and program week.
    /// </summary>
    Task<WeeklyEvaluation?> GetWeeklyEvaluationByEnrollmentAndWeekAsync(long enrollmentId, long programWeekId);

    /// <summary>
    /// Recalculates the attendance score and weekly average for the weekly evaluation matching the given enrollment and program week.
    /// </summary>
    Task RecalculateAttendanceScoreAsync(long enrollmentId, long programWeekId, Guid presentStatusId);

    /// <summary>
    /// Retrieves feedback by its identifier.
    /// </summary>
    Task<Feedback?> GetFeedbackByIdAsync(long feedbackId);
    /// <summary>
    /// Retrieves all feedback entries for a given employee.
    /// </summary>
    Task<List<Feedback>> GetFeedbacksByEmployeeAsync(Guid employeeId);
    /// <summary>
    /// Retrieves all feedback entries for a given weekly evaluation.
    /// </summary>
    Task<List<Feedback>> GetFeedbacksByWeeklyEvaluationAsync(long weeklyEvaluationId);
    /// <summary>
    /// Retrieves all feedback entries.
    /// </summary>
    Task<List<Feedback>> GetAllFeedbacksAsync();
    /// <summary>
    /// Creates a new feedback entry.
    /// </summary>
    Task CreateFeedbackAsync(Feedback entity);
    /// <summary>
    /// Updates an existing feedback entry.
    /// </summary>
    void UpdateFeedback(Feedback entity);
    /// <summary>
    /// Deletes a feedback entry.
    /// </summary>
    void DeleteFeedback(Feedback entity);

    /// <summary>
    /// Retrieves the leaderboard entry for a given enrollment.
    /// </summary>
    Task<Leaderboard?> GetLeaderboardByEnrollmentAsync(long enrollmentId);
    /// <summary>
    /// Retrieves all leaderboard entries.
    /// </summary>
    Task<List<Leaderboard>> GetAllLeaderboardsAsync();
    /// <summary>
    /// Creates a new leaderboard entry.
    /// </summary>
    Task CreateLeaderboardAsync(Leaderboard entity);
    /// <summary>
    /// Updates an existing leaderboard entry.
    /// </summary>
    void UpdateLeaderboard(Leaderboard entity);
    /// <summary>
    /// Deletes a leaderboard entry.
    /// </summary>
    void DeleteLeaderboard(Leaderboard item);

    /// <summary>
    /// Retrieves all assessment evaluations.
    /// </summary>
    Task<List<AssessmentEvaluation>> GetAllAssessmentEvaluationsAsync();

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task<int> SaveAsync();
}
