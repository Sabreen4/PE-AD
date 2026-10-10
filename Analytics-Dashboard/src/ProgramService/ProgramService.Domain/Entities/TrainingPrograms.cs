using Shared.Common.Entities;

namespace ProgramService.Domain.Entities
{
    /// <summary>
    /// Represents a training program containing weeks, assignments, and enrollments.
    /// </summary>
    public class TrainingPrograms : BaseEntity
    {
        public long Id { get; set; }

        public long OrganizationId { get; set; }

        public Guid EvaluatorId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DurationWeeks { get; set; }

        public Guid? StatusId { get; set; }

        public ICollection<ProgramWeek> ProgramWeeks { get; set; } = new List<ProgramWeek>();

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
