using Shared.Common.Entities;

namespace ProgramService.Domain.Entities
{
    /// <summary>
    /// Represents a single week within a program.
    /// </summary>
    public class ProgramWeek : BaseEntity
    {
        public long Id { get; set; }

        public long TrainingProgramId { get; set; }

        public int WeekNumber { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsLocked { get; set; }

        public string? VideoName { get; set; }
        public string? VideoPath { get; set; }
        public long? VideoSize { get; set; }
        public TimeSpan? VideoDuration { get; set; }
        public string? ThumbnailPath { get; set; }

        public TrainingPrograms TrainingPrograms { get; set; } = null!;

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
