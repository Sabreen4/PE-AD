using EvaluationService.Application.Contracts;
using EvaluationService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Application.Services;

/// <summary>
/// Implements evaluation operations for attendance, weekly evaluations, feedback, leaderboard, and assessments.
/// </summary>
public class EvaluationService : IEvaluationService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly ILogger<EvaluationService> _logger;

    public EvaluationService(IRepoWrapper repoWrapper, ILogger<EvaluationService> logger)
    {
        _repoWrapper = repoWrapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all attendance records for a given enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <returns>A list of attendance records.</returns>
    public async Task<List<Attendance>> GetAttendancesByEnrollmentAsync(long enrollmentId)
    {
        _logger.LogInformation("Getting attendances for enrollment {EnrollmentId}", enrollmentId);
        return await Task.FromResult(_repoWrapper.Attendance.FindByCondition(a => a.EnrollmentId == enrollmentId).ToList());
    }

    /// <summary>
    /// Retrieves attendance records for a given enrollment and program week.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="programWeekId">The program week identifier.</param>
    /// <returns>A list of attendance records.</returns>
    public async Task<List<Attendance>> GetAttendancesByEnrollmentAndWeekAsync(long enrollmentId, long programWeekId)
    {
        _logger.LogInformation("Getting attendances for enrollment {EnrollmentId}, week {ProgramWeekId}", enrollmentId, programWeekId);
        return await Task.FromResult(_repoWrapper.Attendance
            .FindByCondition(a => a.EnrollmentId == enrollmentId && a.ProgramWeekId == programWeekId)
            .ToList());
    }

    /// <summary>
    /// Checks whether an attendance record already exists for the given enrollment, program week, and date.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="programWeekId">The program week identifier.</param>
    /// <param name="attendanceDate">The attendance date.</param>
    /// <returns>True if a record exists; otherwise false.</returns>
    public async Task<bool> AttendanceExistsForDateAsync(long enrollmentId, long programWeekId, DateOnly attendanceDate)
    {
        _logger.LogInformation("Checking attendance for enrollment {EnrollmentId}, week {ProgramWeekId}, date {Date}", enrollmentId, programWeekId, attendanceDate);
        return await _repoWrapper.Attendance.AnyAsync(a =>
            a.EnrollmentId == enrollmentId && a.ProgramWeekId == programWeekId && a.AttendanceDate == attendanceDate);
    }

    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    /// <param name="entity">The attendance entity to create.</param>
    public async Task CreateAttendanceAsync(Attendance entity)
    {
        _logger.LogInformation("Creating attendance for enrollment {EnrollmentId}", entity.EnrollmentId);
        await _repoWrapper.Attendance.CreateAsync(entity);
    }

    /// <summary>
    /// Updates an existing attendance record.
    /// </summary>
    /// <param name="entity">The attendance entity to update.</param>
    public async Task UpdateAttendanceAsync(Attendance entity)
    {
        _logger.LogInformation("Updating attendance {Id}", entity.Id);
        _repoWrapper.Attendance.Update(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an attendance record.
    /// </summary>
    /// <param name="entity">The attendance entity to delete.</param>
    public async Task DeleteAttendanceAsync(Attendance entity)
    {
        _logger.LogInformation("Deleting attendance {Id}", entity.Id);
        _repoWrapper.Attendance.Delete(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves an attendance record by its identifier.
    /// </summary>
    /// <param name="attendanceId">The attendance identifier.</param>
    /// <returns>The attendance entity if found; otherwise null.</returns>
    public async Task<Attendance?> GetAttendanceByIdAsync(long attendanceId)
    {
        _logger.LogInformation("Getting attendance by id {Id}", attendanceId);
        return await _repoWrapper.Attendance.GetByIdAsync(attendanceId);
    }

    /// <summary>
    /// Retrieves all weekly evaluations.
    /// </summary>
    /// <returns>A list of all weekly evaluations.</returns>
    public async Task<List<WeeklyEvaluation>> GetAllWeeklyEvaluationsAsync()
    {
        _logger.LogInformation("Getting all weekly evaluations");
        return await Task.FromResult(_repoWrapper.WeeklyEvaluation.FindAll().ToList());
    }

    /// <summary>
    /// Retrieves a weekly evaluation by its identifier.
    /// </summary>
    /// <param name="weeklyEvaluationId">The weekly evaluation identifier.</param>
    /// <returns>The weekly evaluation entity if found; otherwise null.</returns>
    public async Task<WeeklyEvaluation?> GetWeeklyEvaluationByIdAsync(long weeklyEvaluationId)
    {
        _logger.LogInformation("Getting weekly evaluation by id {Id}", weeklyEvaluationId);
        return await _repoWrapper.WeeklyEvaluation.GetByIdAsync(weeklyEvaluationId);
    }

    /// <summary>
    /// Checks whether a weekly evaluation already exists for the given enrollment and program week (ignoring query filters).
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="programWeekId">The program week identifier.</param>
    /// <returns>True if a record exists; otherwise false.</returns>
    public async Task<bool> WeeklyEvaluationExistsAsync(long enrollmentId, long programWeekId)
    {
        _logger.LogInformation("Checking weekly evaluation existence for enrollment {EnrollmentId}, week {ProgramWeekId}", enrollmentId, programWeekId);
        return await _repoWrapper.WeeklyEvaluation.AnyAsync(w =>
            w.EnrollmentId == enrollmentId && w.ProgramWeekId == programWeekId,
            ignoreQueryFilters: true);
    }

    /// <summary>
    /// Creates a new weekly evaluation.
    /// </summary>
    /// <param name="entity">The weekly evaluation entity to create.</param>
    public async Task CreateWeeklyEvaluationAsync(WeeklyEvaluation entity)
    {
        _logger.LogInformation("Creating weekly evaluation for week {ProgramWeekId}, assignment {EmployeeAssignmentId}", entity.ProgramWeekId, entity.EmployeeAssignmentId);
        await _repoWrapper.WeeklyEvaluation.CreateAsync(entity);
    }

    /// <summary>
    /// Updates an existing weekly evaluation.
    /// </summary>
    /// <param name="entity">The weekly evaluation entity to update.</param>
    public async Task UpdateWeeklyEvaluationAsync(WeeklyEvaluation entity)
    {
        _logger.LogInformation("Updating weekly evaluation {Id}", entity.Id);
        _repoWrapper.WeeklyEvaluation.Update(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves weekly evaluations for a given enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <returns>A list of weekly evaluations.</returns>
    public async Task<List<WeeklyEvaluation>> GetWeeklyEvaluationsByEnrollmentAsync(long enrollmentId)
    {
        _logger.LogInformation("Getting weekly evaluations for enrollment {EnrollmentId}", enrollmentId);
        return await Task.FromResult(_repoWrapper.WeeklyEvaluation.FindByCondition(w => w.EnrollmentId == enrollmentId).ToList());
    }

    public async Task<WeeklyEvaluation?> GetWeeklyEvaluationByEnrollmentAndWeekAsync(long enrollmentId, long programWeekId)
    {
        _logger.LogInformation("Getting weekly evaluation for enrollment {EnrollmentId}, week {ProgramWeekId}", enrollmentId, programWeekId);
        return await Task.FromResult(_repoWrapper.WeeklyEvaluation
            .FindByCondition(w => w.EnrollmentId == enrollmentId && w.ProgramWeekId == programWeekId)
            .FirstOrDefault());
    }

    /// <summary>
    /// Retrieves feedback by its identifier.
    /// </summary>
    /// <param name="feedbackId">The feedback identifier.</param>
    /// <returns>The feedback entity if found; otherwise null.</returns>
    public async Task<Feedback?> GetFeedbackByIdAsync(long feedbackId)
    {
        _logger.LogInformation("Getting feedback by id {Id}", feedbackId);
        return await _repoWrapper.Feedback.GetByIdAsync(feedbackId);
    }

    /// <summary>
    /// Retrieves all feedback entries for a given employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <returns>A list of feedback entries.</returns>
    public async Task<List<Feedback>> GetFeedbacksByEmployeeAsync(Guid employeeId)
    {
        _logger.LogInformation("Getting feedbacks for employee {EmployeeId}", employeeId);
        return await Task.FromResult(_repoWrapper.Feedback
            .FindByCondition(f => f.EmployeeId == employeeId)
            .ToList());
    }

    /// <summary>
    /// Retrieves all feedback entries for a given weekly evaluation.
    /// </summary>
    /// <param name="weeklyEvaluationId">The weekly evaluation identifier.</param>
    /// <returns>A list of feedback entries.</returns>
    public async Task<List<Feedback>> GetFeedbacksByWeeklyEvaluationAsync(long weeklyEvaluationId)
    {
        _logger.LogInformation("FEEDBACK DEBUG: EvaluationService querying feedbacks for WeeklyEvaluationId={WeeklyEvaluationId}", weeklyEvaluationId);
        List<Feedback> result = _repoWrapper.Feedback
            .FindByCondition(f => f.WeeklyEvaluationId == weeklyEvaluationId)
            .ToList();
        _logger.LogInformation("FEEDBACK DEBUG: EvaluationService found {Count} feedback records for WeeklyEvaluationId={WeeklyEvaluationId}", result.Count, weeklyEvaluationId);
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Retrieves all feedback entries.
    /// </summary>
    /// <returns>A list of all feedback entries.</returns>
    public async Task<List<Feedback>> GetAllFeedbacksAsync()
    {
        _logger.LogInformation("Getting all feedbacks");
        return await Task.FromResult(_repoWrapper.Feedback.FindAll().ToList());
    }

    /// <summary>
    /// Creates a new feedback entry.
    /// </summary>
    /// <param name="entity">The feedback entity to create.</param>
    public async Task CreateFeedbackAsync(Feedback entity)
    {
        _logger.LogInformation("Creating feedback for employee {EmployeeId}, weekly evaluation {WeeklyEvaluationId}", entity.EmployeeId, entity.WeeklyEvaluationId);
        await _repoWrapper.Feedback.CreateAsync(entity);
    }

    /// <summary>
    /// Updates an existing feedback entry.
    /// </summary>
    /// <param name="entity">The feedback entity to update.</param>
    public void UpdateFeedback(Feedback entity)
    {
        _logger.LogInformation("Updating feedback {Id}", entity.Id);
        _repoWrapper.Feedback.Update(entity);
    }

    /// <summary>
    /// Deletes a feedback entry.
    /// </summary>
    /// <param name="entity">The feedback entity to delete.</param>
    public void DeleteFeedback(Feedback entity)
    {
        _logger.LogInformation("Deleting feedback {Id}", entity.Id);
        _repoWrapper.Feedback.Delete(entity);
    }

    /// <summary>
    /// Retrieves the leaderboard entry for a given enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <returns>The leaderboard entry if found; otherwise null.</returns>
    public async Task<Leaderboard?> GetLeaderboardByEnrollmentAsync(long enrollmentId)
    {
        _logger.LogInformation("Getting leaderboard for enrollment {EnrollmentId}", enrollmentId);
        return await Task.FromResult(_repoWrapper.Leaderboard
            .FindByCondition(l => l.EnrollmentId == enrollmentId)
            .FirstOrDefault());
    }

    /// <summary>
    /// Retrieves all leaderboard entries.
    /// </summary>
    /// <returns>A list of all leaderboard entries.</returns>
    public async Task<List<Leaderboard>> GetAllLeaderboardsAsync()
    {
        _logger.LogInformation("Getting all leaderboards");
        return await Task.FromResult(_repoWrapper.Leaderboard.FindAll().ToList());
    }

    /// <summary>
    /// Creates a new leaderboard entry.
    /// </summary>
    /// <param name="entity">The leaderboard entity to create.</param>
    public async Task CreateLeaderboardAsync(Leaderboard entity)
    {
        _logger.LogInformation("Creating leaderboard entry for enrollment {EnrollmentId}", entity.EnrollmentId);
        await _repoWrapper.Leaderboard.CreateAsync(entity);
    }

    /// <summary>
    /// Updates an existing leaderboard entry.
    /// </summary>
    /// <param name="entity">The leaderboard entity to update.</param>
    public void UpdateLeaderboard(Leaderboard entity)
    {
        _logger.LogInformation("Updating leaderboard entry {Id}", entity.Id);
        _repoWrapper.Leaderboard.Update(entity);
    }

    /// <summary>
    /// Deletes a leaderboard entry.
    /// </summary>
    /// <param name="item">The leaderboard entity to delete.</param>
    public void DeleteLeaderboard(Leaderboard item)
    {
        _logger.LogInformation("Deleting leaderboard entry {Id}", item.Id);
        _repoWrapper.Leaderboard.Delete(item);
    }

    /// <summary>
    /// Retrieves all assessment evaluations.
    /// </summary>
    /// <returns>A list of all assessment evaluations.</returns>
    public async Task<List<AssessmentEvaluation>> GetAllAssessmentEvaluationsAsync()
    {
        _logger.LogInformation("Getting all assessment evaluations");
        return await Task.FromResult(_repoWrapper.AssessmentEvaluation.FindAll().ToList());
    }

    /// <summary>
    /// Recalculates the attendance score and weekly average for the weekly evaluation matching the given enrollment and program week.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="programWeekId">The program week identifier.</param>
    /// <param name="presentStatusId">The GUID of the Present attendance status ref term.</param>
    public async Task RecalculateAttendanceScoreAsync(long enrollmentId, long programWeekId, Guid presentStatusId)
    {
        _logger.LogInformation("Recalculating attendance score for enrollment {EnrollmentId}, week {ProgramWeekId}", enrollmentId, programWeekId);

        List<Attendance> attendanceRecords = await GetAttendancesByEnrollmentAndWeekAsync(enrollmentId, programWeekId);

        int presentCount = attendanceRecords.Count(a => a.AttendanceStatusId == presentStatusId);
        int totalCount = attendanceRecords.Count;

        decimal attendanceRatio = totalCount > 0
            ? Math.Round((decimal)presentCount / totalCount, 2)
            : 0m;

        int attendanceScore = attendanceRatio > 0.75m ? 3
            : attendanceRatio >= 0.50m ? 2
            : 1;

        List<WeeklyEvaluation> existingEvals = await GetWeeklyEvaluationsByEnrollmentAsync(enrollmentId);
        WeeklyEvaluation? weeklyEval = existingEvals.FirstOrDefault(w => w.ProgramWeekId == programWeekId);

        if (weeklyEval is not null)
        {
            weeklyEval.AttendanceScore = attendanceScore;
            weeklyEval.AttendancePercentage = attendanceRatio;
            weeklyEval.WeeklyAverage = Math.Round(
                (attendanceScore + weeklyEval.EngagementScore + weeklyEval.ApplicationScore) / 3m, 2);

            _repoWrapper.WeeklyEvaluation.Update(weeklyEval);
            _logger.LogInformation("Updated weekly evaluation {Id} with recalculated attendance score {Score}", weeklyEval.Id, attendanceScore);
        }
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveAsync()
    {
        _logger.LogInformation("Saving evaluation service changes");
        return await _repoWrapper.SaveAsync();
    }
}
