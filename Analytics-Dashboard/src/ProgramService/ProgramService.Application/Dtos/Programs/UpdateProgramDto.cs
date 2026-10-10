namespace ProgramService.Application.Dtos.Programs;

/// <summary>
/// Data transfer object for updating an existing training program.
/// </summary>
public class UpdateProgramDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Int32 DurationWeeks { get; set; }
    public string Status { get; set; } = string.Empty;
}

