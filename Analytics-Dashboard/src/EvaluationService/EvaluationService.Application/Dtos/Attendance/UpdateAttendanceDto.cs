namespace EvaluationService.Application.Dtos.Attendance;

/// <summary>
/// DTO for updating an existing attendance record.
/// </summary>
public class UpdateAttendanceDto
{
    public DateOnly? AttendanceDate { get; set; }

    public string? AttendanceStatus { get; set; }
}
