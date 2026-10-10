namespace EvaluationService.Application.Dtos.Attendance;

/// <summary>
/// DTO for creating a new attendance record.
/// </summary>
public class CreateAttendanceDto
{
    public Int64 ProgramWeekId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public string AttendanceStatus { get; set; } = string.Empty;
}
