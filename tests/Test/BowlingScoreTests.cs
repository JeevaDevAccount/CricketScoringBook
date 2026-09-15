using Domain.Aggregates.MatchAggregate;

namespace tests.Test;

public sealed class BowlingScoreTests
{
    [Fact]
    public void RecordDelivery_ShouldUpdateRunsAndBalls()
    {
        var score = BowlingScore.Create(201);

        score.RecordDelivery(
            runsConceded: 4,
            countAsBall: true,
            creditedWithWicket: false);

        Assert.Equal(4, score.RunsConceded);
        Assert.Equal(1, score.Balls);
    }

    [Fact]
    public void WicketCreditedToBowler_ShouldIncrementWickets()
    {
        var score = BowlingScore.Create(201);

        score.RecordDelivery(
            runsConceded: 0,
            countAsBall: true,
            creditedWithWicket: true);

        Assert.Equal(1, score.TotalWickets);
    }

    [Fact]
    public void IllegalDelivery_ShouldNotIncrementBalls()
    {
        var score = BowlingScore.Create(201);

        score.RecordDelivery(
            runsConceded: 1,
            countAsBall: false,
            creditedWithWicket: false);

        Assert.Equal(1, score.RunsConceded);
        Assert.Equal(0, score.Balls);
    }

    [Fact]
    public void Economy_ShouldBeCalculatedCorrectly()
    {
        var score = BowlingScore.Create(201);

        score.RecordDelivery(4, true, false);
        score.RecordDelivery(4, true, false);
        score.RecordDelivery(4, true, false);

        Assert.Equal(24m, score.Economy);
    }

    [Fact]
    public void Maiden_ShouldBeRecordedAndUndone()
    {
        var score = BowlingScore.Create(201);

        score.RecordMaiden();

        Assert.Equal(1, score.Maidens);

        score.UndoMaiden();

        Assert.Equal(0, score.Maidens);
    }

    [Fact]
    public void UndoDelivery_ShouldRestorePreviousScore()
    {
        var score = BowlingScore.Create(201);

        score.RecordDelivery(4, true, true);

        score.UndoDelivery(4, true, true);

        Assert.Equal(0, score.RunsConceded);
        Assert.Equal(0, score.Balls);
        Assert.Equal(0, score.TotalWickets);
    }

    [Fact]
    public void NegativeRuns_ShouldThrow()
    {
        var score = BowlingScore.Create(201);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            score.RecordDelivery(-1, true, false));
    }
}