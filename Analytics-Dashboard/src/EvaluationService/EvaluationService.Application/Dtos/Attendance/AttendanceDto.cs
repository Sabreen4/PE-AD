namespace EvaluationService.Application.Dtos.Attendance;

/// <summary>
/// DTO representing an attendance record.
/// </summary>
public class AttendanceDto
{
    public Int64 Id { get; set; }
    public Int64 ProgramWeekId { get; set; }
    public Int64 EnrollmentId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public Guid AttendanceStatusId { get; set; }
}
