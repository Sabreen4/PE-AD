using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;

namespace ProgramService.Application.Services;

/// <summary>
/// Application service handling program domain operations including
/// training programs, program weeks, assignments, enrollments,
/// and employee assignment management.
/// </summary>
public class ProgramService : IProgramService
{
    private readonly IRepoWrapper _repo;
    private readonly ILogger<ProgramService> _logger;

    public ProgramService(IRepoWrapper repo, ILogger<ProgramService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<TrainingPrograms>> GetAllProgramsAsync()
    {
        _logger.LogInformation("Getting all programs");
        return await Task.FromResult(_repo.Program.FindAll().ToList());
    }

    public async Task<List<long>> GetEnrolledProgramIdsAsync(Guid employeeId)
    {
        _logger.LogInformation("Getting enrolled program IDs for employee {EmployeeId}", employeeId);
        return await Task.FromResult(_repo.Enrollment
            .FindByCondition(e => e.EmployeeId == employeeId && e.IsActive)
            .Select(e => e.TrainingProgramId)
            .ToList());
    }

    public async Task<TrainingPrograms?> GetProgramByIdAsync(long id)
    {
        _logger.LogInformation("Getting program by Id={ProgramId}", id);
        return await _repo.Program.GetByIdAsync(id);
    }

    public async Task CreateProgramAsync(TrainingPrograms program)
    {
        _logger.LogInformation("Creating program: {Name}", program.Name);
        await _repo.Program.CreateAsync(program);
    }

    public void UpdateProgram(TrainingPrograms program)
    {
        _logger.LogInformation("Updating program Id={ProgramId}", program.Id);
        _repo.Program.Update(program);
    }

    public async Task<List<ProgramWeek>> GetProgramWeeksByProgramAsync(long programId)
    {
        _logger.LogInformation("Getting program weeks for program {ProgramId}", programId);
        return await Task.FromResult(_repo.ProgramWeek.FindByCondition(w => w.TrainingProgramId == programId).ToList());
    }

    public async Task<ProgramWeek?> GetProgramWeekByIdAsync(long weekId)
    {
        _logger.LogInformation("Getting program week by Id={WeekId}", weekId);
        return await _repo.ProgramWeek.GetByIdAsync(weekId);
    }

    public async Task<ProgramWeek?> GetProgramWeekByProgramAndIdAsync(long programId, long weekId)
    {
        _logger.LogInformation("Getting program week {WeekId} for program {ProgramId}", weekId, programId);
        return await Task.FromResult(_repo.ProgramWeek.FindByCondition(w => w.Id == weekId && w.TrainingProgramId == programId).FirstOrDefault());
    }

    public async Task<bool> ProgramWeekExistsAsync(long programId, int weekNumber)
    {
        _logger.LogInformation("Checking program week existence for program {ProgramId}, week {WeekNumber}", programId, weekNumber);
        return await _repo.ProgramWeek.AnyAsync(w => w.TrainingProgramId == programId && w.WeekNumber == weekNumber);
    }

    public async Task CreateProgramWeekAsync(ProgramWeek week)
    {
        _logger.LogInformation("Creating program week {WeekNumber} for program {ProgramId}", week.WeekNumber, week.TrainingProgramId);
        await _repo.ProgramWeek.CreateAsync(week);
    }

    public void UpdateProgramWeek(ProgramWeek week)
    {
        _logger.LogInformation("Updating program week Id={WeekId}", week.Id);
        _repo.ProgramWeek.Update(week);
    }

    public async Task<List<Assignment>> GetAssignmentsByWeekAsync(long programId, long programWeekId)
    {
        _logger.LogInformation("Getting assignments for program {ProgramId}, week {ProgramWeekId}", programId, programWeekId);
        return await Task.FromResult(_repo.Assignment.FindByCondition(a => a.TrainingProgramId == programId && a.ProgramWeekId == programWeekId).ToList());
    }

    public async Task<Assignment?> GetAssignmentByIdAsync(long assignmentId)
    {
        _logger.LogInformation("Getting assignment by Id={AssignmentId}", assignmentId);
        return await _repo.Assignment.GetByIdAsync(assignmentId);
    }

    public async Task<Assignment?> GetAssignmentByProgramWeekAndIdAsync(long programId, long programWeekId, long assignmentId)
    {
        _logger.LogInformation("Getting assignment {AssignmentId} for program {ProgramId}, week {ProgramWeekId}", assignmentId, programId, programWeekId);
        return await Task.FromResult(_repo.Assignment.FindByCondition(a => a.Id == assignmentId && a.TrainingProgramId == programId && a.ProgramWeekId == programWeekId).FirstOrDefault());
    }

    public async Task CreateAssignmentAsync(Assignment assignment)
    {
        _logger.LogInformation("Creating assignment: {Title}", assignment.Title);
        await _repo.Assignment.CreateAsync(assignment);
    }

    public void UpdateAssignment(Assignment assignment)
    {
        _logger.LogInformation("Updating assignment Id={AssignmentId}", assignment.Id);
        _repo.Assignment.Update(assignment);
    }

    public async Task<List<Enrollment>> GetEnrollmentsByProgramAsync(long programId)
    {
        _logger.LogInformation("Getting enrollments for program {ProgramId}", programId);
        return await Task.FromResult(_repo.Enrollment.FindByCondition(e => e.TrainingProgramId == programId).ToList());
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(long enrollmentId)
    {
        _logger.LogInformation("Getting enrollment by Id={EnrollmentId}", enrollmentId);
        return await _repo.Enrollment.GetByIdAsync(enrollmentId);
    }

    public async Task<Enrollment?> GetActiveEnrollmentByEmployeeAsync(long programId, Guid employeeId)
    {
        _logger.LogInformation("Getting active enrollment for program {ProgramId}, employee {EmployeeId}", programId, employeeId);
        return await Task.FromResult(_repo.Enrollment.FindByCondition(e => e.TrainingProgramId == programId && e.EmployeeId == employeeId && e.IsActive).FirstOrDefault());
    }

    public async Task<List<Enrollment>> GetEnrollmentsByIdsAsync(List<long> enrollmentIds)
    {
        _logger.LogInformation("Getting {Count} enrollments by IDs", enrollmentIds.Count);
        return await Task.FromResult(_repo.Enrollment.FindByCondition(e => enrollmentIds.Contains(e.Id)).ToList());
    }

    public async Task<List<TrainingPrograms>> GetProgramsByIdsAsync(List<long> programIds)
    {
        _logger.LogInformation("Getting {Count} programs by IDs", programIds.Count);
        return await Task.FromResult(_repo.Program.FindByCondition(p => programIds.Contains(p.Id)).ToList());
    }

    public async Task<List<ProgramWeek>> GetProgramWeeksByIdsAsync(List<long> weekIds)
    {
        _logger.LogInformation("Getting {Count} program weeks by IDs", weekIds.Count);
        return await Task.FromResult(_repo.ProgramWeek.FindByCondition(w => weekIds.Contains(w.Id)).ToList());
    }

    public async Task<bool> IsEmployeeEnrolledAsync(long programId, Guid employeeId)
    {
        _logger.LogInformation("Checking if employee {EmployeeId} is enrolled in program {ProgramId}", employeeId, programId);
        return await _repo.Enrollment.AnyAsync(e => e.TrainingProgramId == programId && e.EmployeeId == employeeId);
    }

    public async Task CreateEnrollmentAsync(Enrollment enrollment)
    {
        _logger.LogInformation("Creating enrollment for program {ProgramId}, employee {EmployeeId}", enrollment.TrainingProgramId, enrollment.EmployeeId);
        await _repo.Enrollment.CreateAsync(enrollment);
    }

    public void DeleteEnrollment(Enrollment enrollment)
    {
        _logger.LogInformation("Deleting enrollment Id={EnrollmentId}", enrollment.Id);
        _repo.Enrollment.Delete(enrollment);
    }

    public async Task<List<EmployeeAssignment>> GetEmployeeAssignmentsByAssignmentAsync(long assignmentId)
    {
        _logger.LogInformation("Getting employee assignments for assignment {AssignmentId}", assignmentId);
        return await Task.FromResult(_repo.EmployeeAssignment.FindByCondition(ea => ea.AssignmentId == assignmentId).ToList());
    }

    public async Task<List<EmployeeAssignment>> GetEmployeeAssignmentsByAssignmentSearchAsync(long assignmentId, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return await GetEmployeeAssignmentsByAssignmentAsync(assignmentId);

        _logger.LogInformation("Getting employee assignments for assignment {AssignmentId} with file name search '{Search}'", assignmentId, search);
        return await Task.FromResult(_repo.EmployeeAssignment
            .FindByCondition(ea => ea.AssignmentId == assignmentId && ea.FileName.Contains(search))
            .ToList());
    }

    public async Task<EmployeeAssignment?> GetEmployeeAssignmentByIdAsync(long employeeAssignmentId)
    {
        _logger.LogInformation("Getting employee assignment by Id={EmployeeAssignmentId}", employeeAssignmentId);
        return await _repo.EmployeeAssignment.GetByIdAsync(employeeAssignmentId);
    }

    public async Task<EmployeeAssignment?> GetEmployeeAssignmentByAssignmentAndIdAsync(long assignmentId, long employeeAssignmentId)
    {
        _logger.LogInformation("Getting employee assignment {EmployeeAssignmentId} for assignment {AssignmentId}", employeeAssignmentId, assignmentId);
        return await Task.FromResult(_repo.EmployeeAssignment.FindByCondition(ea => ea.Id == employeeAssignmentId && ea.AssignmentId == assignmentId).FirstOrDefault());
    }

    public async Task<bool> EmployeeAssignmentExistsAsync(long assignmentId, long enrollmentId)
    {
        _logger.LogInformation("Checking employee assignment existence for assignment {AssignmentId}, enrollment {EnrollmentId}", assignmentId, enrollmentId);
        return await _repo.EmployeeAssignment.AnyAsync(ea => ea.AssignmentId == assignmentId && ea.EnrollmentId == enrollmentId);
    }

    public async Task<EmployeeAssignment?> GetEmployeeAssignmentByAssignmentAndEnrollmentAsync(long assignmentId, long enrollmentId)
    {
        _logger.LogInformation("Getting employee assignment for assignment {AssignmentId}, enrollment {EnrollmentId}", assignmentId, enrollmentId);
        return await Task.FromResult(_repo.EmployeeAssignment.FindByCondition(ea => ea.AssignmentId == assignmentId && ea.EnrollmentId == enrollmentId).FirstOrDefault());
    }

    public async Task CreateEmployeeAssignmentAsync(EmployeeAssignment employeeAssignment)
    {
        _logger.LogInformation("Creating employee assignment for assignment {AssignmentId}, enrollment {EnrollmentId}", employeeAssignment.AssignmentId, employeeAssignment.EnrollmentId);
        await _repo.EmployeeAssignment.CreateAsync(employeeAssignment);
    }

    public void UpdateEmployeeAssignment(EmployeeAssignment employeeAssignment)
    {
        _logger.LogInformation("Updating employee assignment Id={EmployeeAssignmentId}", employeeAssignment.Id);
        _repo.EmployeeAssignment.Update(employeeAssignment);
    }

    public Task UpdateProgramWeekVideoAsync(ProgramWeek week)
    {
        _logger.LogInformation("Updating program week video for week Id={WeekId}", week.Id);
        _repo.ProgramWeek.Update(week);
        return Task.CompletedTask;
    }

    public async Task<int> SaveAsync()
    {
        _logger.LogInformation("Saving program service changes");
        return await _repo.SaveAsync();
    }

    public async Task DeleteProgramAsync(TrainingPrograms program)
    {
        _logger.LogInformation("Deleting program Id={ProgramId} with all child entities", program.Id);

        List<ProgramWeek> weeks = _repo.ProgramWeek
            .FindByCondition(w => w.TrainingProgramId == program.Id)
            .ToList();
        List<Assignment> assignments = _repo.Assignment
            .FindByCondition(a => a.TrainingProgramId == program.Id)
            .ToList();
        List<Enrollment> enrollments = _repo.Enrollment
            .FindByCondition(e => e.TrainingProgramId == program.Id)
            .ToList();
        List<long> assignmentIds = assignments.Select(a => a.Id).ToList();
        List<long> enrollmentIds = enrollments.Select(e => e.Id).ToList();

        List<EmployeeAssignment> employeeAssignments = _repo.EmployeeAssignment
            .FindByCondition(ea => assignmentIds.Contains(ea.AssignmentId) || enrollmentIds.Contains(ea.EnrollmentId))
            .ToList();
        foreach (EmployeeAssignment ea in employeeAssignments)
            _repo.EmployeeAssignment.Delete(ea);

        foreach (Assignment a in assignments)
            _repo.Assignment.Delete(a);

        foreach (ProgramWeek pw in weeks)
            _repo.ProgramWeek.Delete(pw);

        foreach (Enrollment e in enrollments)
            _repo.Enrollment.Delete(e);

        _repo.Program.Delete(program);
    }

    public async Task DeleteProgramWeekAsync(ProgramWeek week)
    {
        _logger.LogInformation("Deleting program week Id={WeekId} with child assignments", week.Id);

        List<Assignment> assignments = _repo.Assignment
            .FindByCondition(a => a.ProgramWeekId == week.Id)
            .ToList();
        List<long> assignmentIds = assignments.Select(a => a.Id).ToList();

        List<EmployeeAssignment> employeeAssignments = _repo.EmployeeAssignment
            .FindByCondition(ea => assignmentIds.Contains(ea.AssignmentId))
            .ToList();
        foreach (EmployeeAssignment ea in employeeAssignments)
            _repo.EmployeeAssignment.Delete(ea);

        foreach (Assignment a in assignments)
            _repo.Assignment.Delete(a);

        _repo.ProgramWeek.Delete(week);
    }
}
