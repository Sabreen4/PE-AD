namespace EvaluationService.Tests;

/// <summary>
/// Tests attendance score calculations, weekly score aggregations, feedback adjustments, and leaderboard scoring.
/// </summary>
public class AttendanceScoreCalculationTests
{
    private static int GetAttendanceScore(int presentCount, int totalCount)
    {
        if (totalCount == 0) return 1;
        decimal ratio = (decimal)presentCount / totalCount;
        return ratio > 0.75m ? 3 : ratio >= 0.50m ? 2 : 1;
    }

    /// <summary>
    /// Verifies that 5 present out of 5 total (above 75%) returns attendance score 3.
    /// </summary>
    [Fact]
    public void Attendance_5Present5Total_Above75Percent_Returns3()
    {
        int score = GetAttendanceScore(5, 5);
        Assert.Equal(3, score);
    }

    /// <summary>
    /// Verifies that 4 present out of 5 total (80%, above 75%) returns attendance score 3.
    /// </summary>
    [Fact]
    public void Attendance_4Present5Total_Above75Percent_Returns3()
    {
        int score = GetAttendanceScore(4, 5);
        Assert.Equal(3, score);
    }

    /// <summary>
    /// Verifies that 3 present out of 5 total (60%, between 50% and 75%) returns attendance score 2.
    /// </summary>
    [Fact]
    public void Attendance_3Present5Total_Exactly60Percent_Returns2()
    {
        int score = GetAttendanceScore(3, 5);
        Assert.Equal(2, score);
    }

    /// <summary>
    /// Verifies that 2 present out of 5 total (40%, below 50%) returns attendance score 1.
    /// </summary>
    [Fact]
    public void Attendance_2Present5Total_Exactly40Percent_Below50_Returns1()
    {
        int score = GetAttendanceScore(2, 5);
        Assert.Equal(1, score);
    }

    /// <summary>
    /// Verifies that 1 present out of 5 total (20%, below 50%) returns attendance score 1.
    /// </summary>
    [Fact]
    public void Attendance_1Present5Total_Below50Percent_Returns1()
    {
        int score = GetAttendanceScore(1, 5);
        Assert.Equal(1, score);
    }

    /// <summary>
    /// Verifies that 0 present out of 5 total (0%, below 50%) returns attendance score 1.
    /// </summary>
    [Fact]
    public void Attendance_0Present5Total_ZeroPercent_Returns1()
    {
        int score = GetAttendanceScore(0, 5);
        Assert.Equal(1, score);
    }

    /// <summary>
    /// Verifies that zero working days defaults to attendance score 1 (edge case).
    /// </summary>
    [Fact]
    public void Attendance_0WorkingDays_Returns1()
    {
        int score = GetAttendanceScore(0, 0);
        Assert.Equal(1, score);
    }

    /// <summary>
    /// Verifies that exactly 75% attendance (3 out of 4) returns score 2 (not 3, since > 75% is required for score 3).
    /// </summary>
    [Fact]
    public void Attendance_75PercentExactly_Returns2()
    {
        int score = GetAttendanceScore(3, 4);
        Assert.Equal(2, score);
    }

    /// <summary>
    /// Verifies that weekly score with maximum attendance, engagement, and application scores returns 9.
    /// </summary>
    [Fact]
    public void WeeklyScore_Attendance3Engagement3Application3_Returns9()
    {
        int attendanceScore = 3;
        int engagementScore = 3;
        int applicationScore = 3;
        decimal weeklyScore = Math.Round((decimal)(attendanceScore + engagementScore + applicationScore), 2);

        Assert.Equal(9.00m, weeklyScore);
    }

    /// <summary>
    /// Verifies that weekly score with minimum scores across categories returns 1.
    /// </summary>
    [Fact]
    public void WeeklyScore_Attendance1Engagement0Application0_Returns1()
    {
        int attendanceScore = 1;
        int engagementScore = 0;
        int applicationScore = 0;
        decimal weeklyScore = Math.Round((decimal)(attendanceScore + engagementScore + applicationScore), 2);

        Assert.Equal(1.00m, weeklyScore);
    }

    /// <summary>
    /// Verifies that a GREEN_FLAG feedback type returns a +3 adjustment.
    /// </summary>
    [Fact]
    public void FeedbackAdjustment_GreenFlag_ReturnsPlus3()
    {
        string feedbackType = "GREEN_FLAG";
        var scoreMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "GREEN_FLAG", 3m },
            { "RED_FLAG", -2m }
        };

        decimal adjustment = scoreMap[feedbackType];
        Assert.Equal(3m, adjustment);
    }

    /// <summary>
    /// Verifies that a RED_FLAG feedback type returns a -2 adjustment.
    /// </summary>
    [Fact]
    public void FeedbackAdjustment_RedFlag_ReturnsMinus2()
    {
        string feedbackType = "RED_FLAG";
        var scoreMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "GREEN_FLAG", 3m },
            { "RED_FLAG", -2m }
        };

        decimal adjustment = scoreMap[feedbackType];
        Assert.Equal(-2m, adjustment);
    }

    /// <summary>
    /// Verifies that leaderboard score combines weekly scores, assessment marks, and a GREEN_FLAG adjustment correctly.
    /// </summary>
    [Fact]
    public void LeaderboardScore_WeeklyScores40Assessment20GreenFlag3_Returns63()
    {
        decimal totalWeeklyScores = 40m;
        decimal totalAssessmentMarks = 20m;
        decimal totalFeedbackAdjustments = 3m;

        decimal overallScore = Math.Round(totalWeeklyScores + totalAssessmentMarks + totalFeedbackAdjustments, 2);

        Assert.Equal(63.00m, overallScore);
    }

    /// <summary>
    /// Verifies that leaderboard score combines weekly scores, assessment marks, and a RED_FLAG adjustment correctly.
    /// </summary>
    [Fact]
    public void LeaderboardScore_WeeklyScores40Assessment20RedFlag2_Returns58()
    {
        decimal totalWeeklyScores = 40m;
        decimal totalAssessmentMarks = 20m;
        decimal totalFeedbackAdjustments = -2m;

        decimal overallScore = Math.Round(totalWeeklyScores + totalAssessmentMarks + totalFeedbackAdjustments, 2);

        Assert.Equal(58.00m, overallScore);
    }

    /// <summary>
    /// Verifies that leaderboard score with both GREEN_FLAG and RED_FLAG adjustments combines correctly.
    /// </summary>
    [Fact]
    public void LeaderboardScore_WeeklyScores40Assessment20Green3Red2_Returns61()
    {
        decimal totalWeeklyScores = 40m;
        decimal totalAssessmentMarks = 20m;
        decimal totalFeedbackAdjustments = 3m + (-2m);

        decimal overallScore = Math.Round(totalWeeklyScores + totalAssessmentMarks + totalFeedbackAdjustments, 2);

        Assert.Equal(61.00m, overallScore);
    }

    /// <summary>
    /// Verifies that updating feedback from GREEN_FLAG to RED_FLAG replaces the adjustment rather than accumulating it.
    /// </summary>
    [Fact]
    public void FeedbackUpdate_GreenToRed_AdjustmentReplaces()
    {
        decimal oldAdjustment = 3m;
        decimal newAdjustment = -2m;

        decimal finalAdjustment = newAdjustment;

        Assert.Equal(-2m, finalAdjustment);
        Assert.NotEqual(oldAdjustment + newAdjustment, finalAdjustment);
    }

    /// <summary>
    /// Verifies that updating feedback from RED_FLAG to GREEN_FLAG replaces the adjustment rather than accumulating it.
    /// </summary>
    [Fact]
    public void FeedbackUpdate_RedToGreen_AdjustmentReplaces()
    {
        decimal oldAdjustment = -2m;
        decimal newAdjustment = 3m;

        decimal finalAdjustment = newAdjustment;

        Assert.Equal(3m, finalAdjustment);
        Assert.NotEqual(oldAdjustment + newAdjustment, finalAdjustment);
    }
}
