using Shared.Common.Entities;

namespace EvaluationService.Domain.Entities
{
    /// <summary>
    /// Represents the weekly evaluation score of an employee for a given program week.
    /// </summary>
    public sealed class WeeklyEvaluation : BaseEntity
    {
        public long Id { get; set; }

        public long ProgramWeekId { get; set; }

        public long EmployeeAssignmentId { get; set; }

        public long EnrollmentId { get; set; }

        public decimal AttendanceScore { get; set; }

        public int EngagementScore { get; set; }

        public int ApplicationScore { get; set; }

        public decimal WeeklyAverage { get; set; }

        public decimal AttendancePercentage { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}