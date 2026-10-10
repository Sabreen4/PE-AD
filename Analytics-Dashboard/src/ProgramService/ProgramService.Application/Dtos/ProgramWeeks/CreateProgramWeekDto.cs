namespace ProgramService.Application.Dtos.ProgramWeeks;

/// <summary>
/// Data transfer object for creating a new program week.
/// </summary>
public class CreateProgramWeekDto
{
    public Int32 WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

