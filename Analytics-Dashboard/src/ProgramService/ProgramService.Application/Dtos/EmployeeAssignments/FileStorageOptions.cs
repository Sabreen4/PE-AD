namespace ProgramService.Application.Dtos.EmployeeAssignments;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string UploadPath { get; set; } = "C:\\upload\\program-service";
    public int MaxFileSizeMB { get; set; } = 50;
}
