using Shared.Common.Entities;

namespace EvaluationService.Domain.Entities
{
    /// <summary>
    /// Represents attendance recorded for an enrollment during a specific program week.
    /// </summary>
    public sealed class Attendance : BaseEntity
    {
        public long Id { get; set; }

        public long EnrollmentId { get; set; }

        public long ProgramWeekId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public Guid AttendanceStatusId { get; set; }
    }
}