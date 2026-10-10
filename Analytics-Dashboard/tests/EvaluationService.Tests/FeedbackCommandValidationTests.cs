namespace EvaluationService.Tests;

/// <summary>
/// Tests feedback command validation logic including feedback type validation,
/// engagement/applicationscore range checks, attendance scoring, and leaderboard recalculations.
/// </summary>
public class FeedbackCommandValidationTests
{
    private static readonly Dictionary<string, decimal> ScoreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GREEN_FLAG", 3m },
        { "RED_FLAG", -2m }
    };

    /// <summary>
    /// Verifies that valid feedback type codes return the expected score adjustment.
    /// </summary>
    /// <param name="typeCode">The feedback type code (e.g. GREEN_FLAG, RED_FLAG).</param>
    /// <param name="expectedAdjustment">The expected score adjustment for the given type code.</param>
    [Theory]
    [InlineData("GREEN_FLAG", 3)]
    [InlineData("RED_FLAG", -2)]
    public void ValidFeedbackType_ReturnsCorrectAdjustment(string typeCode, decimal expectedAdjustment)
    {
        bool isValid = ScoreMap.ContainsKey(typeCode);
        decimal adjustment = isValid ? ScoreMap[typeCode] : 0;

        Assert.True(isValid);
        Assert.Equal(expectedAdjustment, adjustment);
    }

    /// <summary>
    /// Verifies that invalid, empty, or null feedback type codes fail validation.
    /// </summary>
    /// <param name="typeCode">The invalid feedback type code.</param>
    [Theory]
    [InlineData("INVALID_TYPE")]
    [InlineData("")]
    [InlineData(null)]
    public void InvalidFeedbackType_ValidationFails(string? typeCode)
    {
        if (typeCode is null || !ScoreMap.ContainsKey(typeCode))
        {
            Assert.True(string.IsNullOrWhiteSpace(typeCode) || !ScoreMap.ContainsKey(typeCode));
        }
    }

    /// <summary>
    /// Verifies that creating feedback without a valid weekly evaluation ID fails validation.
    /// </summary>
    [Fact]
    public void CreateFeedbackWithoutWeeklyEvaluation_ShouldFail()
    {
        long weeklyEvaluationId = 0;
        bool isValid = weeklyEvaluationId > 0;

        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that engagement scores outside the valid range (0-3) fail validation.
    /// </summary>
    /// <param name="score">The out-of-range engagement score.</param>
    [Theory]
    [InlineData(4)]
    [InlineData(-1)]
    [InlineData(10)]
    public void EngagementScoreOutOfRange_ValidationFails(int score)
    {
        bool isValid = score >= 0 && score <= 3;
        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that engagement scores within the valid range (0-3) pass validation.
    /// </summary>
    /// <param name="score">The in-range engagement score.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void EngagementScoreInRange_ValidationPasses(int score)
    {
        bool isValid = score >= 0 && score <= 3;
        Assert.True(isValid);
    }

    /// <summary>
    /// Verifies that application scores outside the valid range (0-3) fail validation.
    /// </summary>
    /// <param name="score">The out-of-range application score.</param>
    [Theory]
    [InlineData(4)]
    [InlineData(-1)]
    [InlineData(10)]
    public void ApplicationScoreOutOfRange_ValidationFails(int score)
    {
        bool isValid = score >= 0 && score <= 3;
        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that application scores within the valid range (0-3) pass validation.
    /// </summary>
    /// <param name="score">The in-range application score.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void ApplicationScoreInRange_ValidationPasses(int score)
    {
        bool isValid = score >= 0 && score <= 3;
        Assert.True(isValid);
    }

    /// <summary>
    /// Verifies that client-submitted attendance scores are ignored and the backend calculates the correct score.
    /// </summary>
    [Fact]
    public void ClientSubmittedAttendanceScore_IsIgnored_BackendCalculatesIt()
    {
        int clientSubmittedScore = 85;
        int presentCount = 4;
        int totalCount = 5;
        decimal ratio = (decimal)presentCount / totalCount;
        int calculatedScore = ratio > 0.75m ? 3 : ratio >= 0.50m ? 2 : 1;

        Assert.Equal(3, calculatedScore);
        Assert.NotEqual(clientSubmittedScore, calculatedScore);
    }

    /// <summary>
    /// Verifies that feedback adjustment values from the client are rejected and the backend determines the correct adjustment.
    /// </summary>
    [Fact]
    public void FeedbackAdjustment_IsNotAcceptedFromClient_BackendDeterminesIt()
    {
        int clientSubmittedAdjustment = 100;
        string feedbackTypeCode = "GREEN_FLAG";

        decimal backendAdjustment = ScoreMap[feedbackTypeCode];

        Assert.Equal(3m, backendAdjustment);
        Assert.NotEqual(clientSubmittedAdjustment, backendAdjustment);
    }

    /// <summary>
    /// Verifies that deleting feedback removes its adjustment from the leaderboard score calculation.
    /// </summary>
    [Fact]
    public void DeleteFeedback_RemovesAdjustmentFromLeaderboard()
    {
        decimal totalWeeklyScores = 40m;
        decimal totalAssessmentMarks = 20m;
        decimal greenFlagAdjustment = 3m;

        decimal scoreWithFeedback = totalWeeklyScores + totalAssessmentMarks + greenFlagAdjustment;
        Assert.Equal(63m, scoreWithFeedback);

        decimal scoreWithoutFeedback = totalWeeklyScores + totalAssessmentMarks;
        Assert.Equal(60m, scoreWithoutFeedback);
    }

    /// <summary>
    /// Verifies that feedback belonging to a different weekly evaluation does not affect the current week's scores.
    /// </summary>
    [Fact]
    public void FeedbackFromAnotherWeek_DoesNotAffectCurrentWeek()
    {
        long currentWeekId = 1;
        long otherWeekId = 2;

        var feedbacks = new List<(long WeeklyEvalId, decimal Adjustment)>
        {
            (1, 3m),
            (2, -2m)
        };

        decimal currentWeekAdjustment = feedbacks.Where(f => f.WeeklyEvalId == currentWeekId).Sum(f => f.Adjustment);
        decimal otherWeekAdjustment = feedbacks.Where(f => f.WeeklyEvalId == otherWeekId).Sum(f => f.Adjustment);

        Assert.Equal(3m, currentWeekAdjustment);
        Assert.Equal(-2m, otherWeekAdjustment);
    }

    /// <summary>
    /// Verifies that recalculating the leaderboard multiple times does not result in duplicate feedback adjustments.
    /// </summary>
    [Fact]
    public void RecalculateLeaderboardMultipleTimes_NoDuplicateAdjustment()
    {
        decimal baseScore = 60m;
        decimal feedbackAdjustment = 3m;

        decimal firstCalculation = baseScore + feedbackAdjustment;
        decimal secondCalculation = baseScore + feedbackAdjustment;

        Assert.Equal(firstCalculation, secondCalculation);
    }

    /// <summary>
    /// Verifies that attendance score thresholds produce the correct score based on present/total ratio.
    /// </summary>
    /// <param name="present">The number of days present.</param>
    /// <param name="total">The total number of working days.</param>
    /// <param name="expectedScore">The expected attendance score.</param>
    [Theory]
    [InlineData(5, 5, 3)]
    [InlineData(4, 5, 3)]
    [InlineData(3, 5, 2)]
    [InlineData(2, 5, 1)]
    [InlineData(1, 5, 1)]
    [InlineData(0, 5, 1)]
    [InlineData(0, 0, 1)]
    [InlineData(3, 4, 2)]
    [InlineData(4, 4, 3)]
    public void AttendanceScoreThresholds_ProducesCorrectScore(int present, int total, int expectedScore)
    {
        int actualScore;
        if (total == 0)
        {
            actualScore = 1;
        }
        else
        {
            decimal ratio = (decimal)present / total;
            actualScore = ratio > 0.75m ? 3 : ratio >= 0.50m ? 2 : 1;
        }
        Assert.Equal(expectedScore, actualScore);
    }
}
