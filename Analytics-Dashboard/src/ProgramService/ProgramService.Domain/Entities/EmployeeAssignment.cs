using Shared.Common.Entities;

namespace ProgramService.Domain.Entities
{
    /// <summary>
    /// Represents an employee's submission/status against a specific assignment within their enrollment.
    /// </summary>
    public class EmployeeAssignment : BaseEntity
    {
        public long Id { get; set; }

       
        public long AssignmentId { get; set; }

       
        public long EnrollmentId { get; set; }

        public Guid? StatusId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public decimal? Marks { get; set; }

        public string? ReviewerComment { get; set; }

        
        public Assignment Assignment { get; set; } = null!;

        public Enrollment Enrollment { get; set; } = null!;
    }
}
