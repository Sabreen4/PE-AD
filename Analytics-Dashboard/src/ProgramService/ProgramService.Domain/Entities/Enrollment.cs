using Shared.Common.Entities;

namespace ProgramService.Domain.Entities
{
    /// <summary>
    /// Represents an employee's enrollment into a program.
    /// </summary>
    public class Enrollment : BaseEntity
    {
        public long Id { get; set; }

        public long TrainingProgramId { get; set; }

        public Guid EmployeeId { get; set; }

        public int CurrentWeek { get; set; }

        public Guid? StatusId { get; set; }

       
        public TrainingPrograms TrainingPrograms { get; set; } = null!;

        public ICollection<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>();
    }
}
