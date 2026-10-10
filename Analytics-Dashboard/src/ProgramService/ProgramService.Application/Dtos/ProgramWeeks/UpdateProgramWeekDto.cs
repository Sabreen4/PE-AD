namespace ProgramService.Application.Dtos.ProgramWeeks;

/// <summary>
/// Data transfer object for updating an existing program week.
/// </summary>
public class UpdateProgramWeekDto
{
    public Int32 WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

