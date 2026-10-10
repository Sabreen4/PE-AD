using EvaluationService.Application.Dtos.Common;

namespace EvaluationService.Application.Dtos.Attendance;

/// <summary>
/// Response DTO containing a paginated list of attendance records.
/// </summary>
public class AttendanceListResponseDto
{
    public List<AttendanceDto> Data { get; set; } = new();
    public PaginationMetadataDto Pagination { get; set; } = default!;
}
