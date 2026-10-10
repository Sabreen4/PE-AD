using Shared.Common.Entities;

namespace ProgramService.Domain.Entities
{
    /// <summary>
    /// Represents an assignment tied to a program and optionally a specific program week.
    /// </summary>
    public class Assignment : BaseEntity
    {
        public long Id { get; set; }

       
        public long TrainingProgramId { get; set; }

        public long? ProgramWeekId { get; set; }

        public Guid? FileTypeId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

       

        public int MaxMarks { get; set; }

       
        public TrainingPrograms TrainingPrograms { get; set; } = null!;

        public ProgramWeek? ProgramWeek { get; set; }

        public ICollection<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>();
    }
}
