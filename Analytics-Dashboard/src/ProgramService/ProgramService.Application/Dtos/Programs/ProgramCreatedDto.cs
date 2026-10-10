namespace ProgramService.Application.Dtos.Programs;

/// <summary>
/// Data transfer object returned after a program is created.
/// </summary>
public class ProgramCreatedDto
{
    public Int64 Id { get; set; }
    public Int64 OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
}

