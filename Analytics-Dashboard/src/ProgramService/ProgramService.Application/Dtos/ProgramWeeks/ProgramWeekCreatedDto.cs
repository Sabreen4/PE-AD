namespace ProgramService.Application.Dtos.ProgramWeeks;

/// <summary>
/// Data transfer object returned after a program week is created.
/// </summary>
public class ProgramWeekCreatedDto
{
    public Int64 Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

