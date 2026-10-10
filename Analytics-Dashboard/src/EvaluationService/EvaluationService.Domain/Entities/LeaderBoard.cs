using Shared.Common.Entities;

namespace EvaluationService.Domain.Entities
{
    /// <summary>
    /// Represents the overall ranking of an enrollment within a program.
    /// </summary>
    public sealed class Leaderboard : BaseEntity
    {
        public long Id { get; set; }

        public long EnrollmentId { get; set; }

        public decimal OverallScore { get; set; }

        public int Rank { get; set; }
    }
}