namespace ProgramService.Application.Dtos.EmployeeAssignments;

public class AssignmentFileDownloadResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
