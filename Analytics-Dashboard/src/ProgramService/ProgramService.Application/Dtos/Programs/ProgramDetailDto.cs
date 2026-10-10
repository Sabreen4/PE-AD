namespace ProgramService.Application.Dtos.Programs;

/// <summary>
/// Detailed data transfer object for a training program.
/// </summary>
public class ProgramDetailDto
{
    public Int64 OrganizationId { get; set; }
    public Guid EvaluatorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Int32 DurationWeeks { get; set; }
    public string? Status { get; set; }
}

