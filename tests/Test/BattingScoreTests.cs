using Domain.Aggregates.MatchAggregate;

namespace tests.Test;

public sealed class BattingScoreTests
{
    [Fact]
    public void RecordDelivery_ShouldUpdateRunsAndBalls()
    {
        var score = BattingScore.Create(101);

        score.RecordDelivery(
            batterRuns: 2,
            countAsBall: true,
            dismissal: null);

        Assert.Equal(2, score.Runs);
        Assert.Equal(1, score.Balls);
    }

    [Fact]
    public void RecordFour_ShouldIncrementFourCount()
    {
        var score = BattingScore.Create(101);

        score.RecordDelivery(4, true, null);

        Assert.Equal(4, score.Runs);
        Assert.Equal(1, score.Fours);
        Assert.Equal(0, score.Sixes);
    }

    [Fact]
    public void RecordSix_ShouldIncrementSixCount()
    {
        var score = BattingScore.Create(101);

        score.RecordDelivery(6, true, null);

        Assert.Equal(6, score.Runs);
        Assert.Equal(1, score.Sixes);
        Assert.Equal(0, score.Fours);
    }

    [Fact]
    public void IllegalDelivery_ShouldNotIncrementBalls()
    {
        var score = BattingScore.Create(101);

        score.RecordDelivery(
            batterRuns: 2,
            countAsBall: false,
            dismissal: null);

        Assert.Equal(2, score.Runs);
        Assert.Equal(0, score.Balls);
    }

    [Fact]
    public void StrikeRate_ShouldBeCalculatedCorrectly()
    {
        var score = BattingScore.Create(101);

        for (int i = 0; i < 5; i++)
        {
            score.RecordDelivery(1, true, null);
        }

        Assert.Equal(100m, score.StrikeRate);
    }

    [Fact]
    public void RecordDismissal_ShouldSetDismissal()
    {
        var score = BattingScore.Create(101);

        var dismissal = Dismissal.Create(
            Domain.Enum.WicketType.Bowled,
            null);

        score.RecordDelivery(10, true, dismissal);

        Assert.NotNull(score.Dismissal);
        Assert.Equal(
            Domain.Enum.WicketType.Bowled,
            score.Dismissal.WicketType);
    }

    [Fact]
    public void UndoDelivery_ShouldRestorePreviousScore()
    {
        var score = BattingScore.Create(101);

        score.RecordDelivery(4, true, null);

        score.UndoDelivery(4, true, null);

        Assert.Equal(0, score.Runs);
        Assert.Equal(0, score.Balls);
        Assert.Equal(0, score.Fours);
    }

    [Fact]
    public void NegativeRuns_ShouldThrow()
    {
        var score = BattingScore.Create(101);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            score.RecordDelivery(-1, true, null));
    }
}