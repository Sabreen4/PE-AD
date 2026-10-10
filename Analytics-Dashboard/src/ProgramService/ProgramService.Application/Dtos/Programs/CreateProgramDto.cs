namespace ProgramService.Application.Dtos.Programs;

/// <summary>
/// Data transfer object for creating a new training program.
/// </summary>
public class CreateProgramDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Int32 DurationWeeks { get; set; }
    public string Status { get; set; } = string.Empty;
}

