using Domain.Aggregates.MatchAggregate;
using Domain.Enum;

namespace tests.Test;

public sealed class InningsTests
{
    private static Innings CreateInnings(
        int maxOvers = 10,
        int? targetRuns = null)
    {
        return Innings.Create(
            inningsNumber: 1,
            battingTeamId: 1,
            bowlingTeamId: 2,
            type: InningsType.Regular,
            superOverNumber: 0,
            maxOvers: maxOvers,
            targetRuns: targetRuns,
            currentStrikerId: 101,
            currentNonStrikerId: 102,
            currentBowlerId: 201);
    }

    private static Match.DeliveryInput Delivery(
        int strikerId = 101,
        int nonStrikerId = 102,
        int bowlerId = 201,
        int batterRuns = 0,
        int totalRuns = 0,
        ExtraType extraType = ExtraType.None,
        Dismissal dismissal = null!,
        int? dismissedPlayerId = null)
    {
        return new Match.DeliveryInput(
            strikerId,
            nonStrikerId,
            bowlerId,
            batterRuns,
            totalRuns,
            extraType,
            dismissal,
            dismissedPlayerId);
    }

    [Fact]
    public void Create_ShouldStartWithOneOver()
    {
        var innings = CreateInnings();

        Assert.Single(innings.Overs);
        Assert.False(innings.Overs.First().IsCompleted);
        Assert.Equal(0, innings.TotalRuns);
        Assert.Equal(0, innings.TotalBalls);
        Assert.Equal(0, innings.Wickets);
        Assert.False(innings.IsCompleted);
    }

    [Fact]
    public void RecordDelivery_ShouldUpdateInningsScore()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(batterRuns: 4, totalRuns: 4));

        Assert.Equal(4, innings.TotalRuns);
        Assert.Equal(1, innings.TotalBalls);
    }

    [Fact]
    public void RecordDelivery_ShouldUpdateBattingScore()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(batterRuns: 4, totalRuns: 4));

        var score = Assert.Single(innings.BattingScores);

        Assert.Equal(101, score.PlayerId);
        Assert.Equal(4, score.Runs);
        Assert.Equal(1, score.Balls);
        Assert.Equal(1, score.Fours);
    }

    [Fact]
    public void RecordDelivery_ShouldUpdateBowlingScore()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(batterRuns: 4, totalRuns: 4));

        var score = Assert.Single(innings.BowlingScores);

        Assert.Equal(201, score.PlayerId);
        Assert.Equal(4, score.RunsConceded);
        Assert.Equal(1, score.Balls);
    }

    [Fact]
    public void Wide_ShouldIncreaseRunsButNotLegalBalls()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(
                batterRuns: 0,
                totalRuns: 1,
                extraType: ExtraType.Wide));

        Assert.Equal(1, innings.TotalRuns);
        Assert.Equal(0, innings.TotalBalls);

        var bowlingScore = Assert.Single(innings.BowlingScores);

        Assert.Equal(1, bowlingScore.RunsConceded);
        Assert.Equal(0, bowlingScore.Balls);
    }

    [Fact]
    public void NoBall_ShouldIncreaseRunsButNotLegalBalls()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(
                batterRuns: 0,
                totalRuns: 1,
                extraType: ExtraType.NoBall));

        Assert.Equal(1, innings.TotalRuns);
        Assert.Equal(0, innings.TotalBalls);

        var bowlingScore = Assert.Single(innings.BowlingScores);

        Assert.Equal(1, bowlingScore.RunsConceded);
        Assert.Equal(0, bowlingScore.Balls);
    }

    [Fact]
    public void OddRuns_ShouldRotateStrike()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(
                strikerId: 101,
                nonStrikerId: 102,
                batterRuns: 1,
                totalRuns: 1));

        Assert.Equal(102, innings.CurrentStrikerId);
        Assert.Equal(101, innings.CurrentNonStrikerId);
    }

    [Fact]
    public void EvenRuns_ShouldNotRotateStrike()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(
                strikerId: 101,
                nonStrikerId: 102,
                batterRuns: 2,
                totalRuns: 2));

        Assert.Equal(101, innings.CurrentStrikerId);
        Assert.Equal(102, innings.CurrentNonStrikerId);
    }

    [Fact]
    public void SixLegalBalls_ShouldCreateNextOver()
    {
        var innings = CreateInnings();

        for (int i = 0; i < 6; i++)
        {
            innings.RecordDelivery(
                Delivery(
                    strikerId: innings.CurrentStrikerId,
                    nonStrikerId: innings.CurrentNonStrikerId));
        }

        Assert.Equal(6, innings.TotalBalls);
        Assert.Equal(2, innings.Overs.Count);

        var completedOver = innings.Overs.First();

        Assert.True(completedOver.IsCompleted);
    }

    [Fact]
    public void Wide_ShouldNotCreateNextOver()
    {
        var innings = CreateInnings();

        for (int i = 0; i < 5; i++)
        {
            innings.RecordDelivery(
                Delivery(
                    strikerId: innings.CurrentStrikerId,
                    nonStrikerId: innings.CurrentNonStrikerId));
        }

        innings.RecordDelivery(
            Delivery(
                strikerId: innings.CurrentStrikerId,
                nonStrikerId: innings.CurrentNonStrikerId,
                totalRuns: 1,
                extraType: ExtraType.Wide));

        Assert.Equal(5, innings.TotalBalls);
        Assert.Single(innings.Overs);
    }

    [Fact]
    public void TargetReached_ShouldCompleteInnings()
    {
        var innings = CreateInnings(targetRuns: 4);

        innings.RecordDelivery(
            Delivery(batterRuns: 4, totalRuns: 4));

        Assert.True(innings.IsCompleted);
        Assert.Equal(4, innings.TotalRuns);
    }

    [Fact]
    public void MaxOversReached_ShouldCompleteInnings()
    {
        var innings = CreateInnings(maxOvers: 1);

        for (int i = 0; i < 6; i++)
        {
            innings.RecordDelivery(
                Delivery(
                    strikerId: innings.CurrentStrikerId,
                    nonStrikerId: innings.CurrentNonStrikerId));
        }

        Assert.True(innings.IsCompleted);
        Assert.Equal(6, innings.TotalBalls);
    }

    [Fact]
    public void TenthWicket_ShouldCompleteInnings()
    {
        var innings = CreateInnings();

        for (int i = 0; i < 10; i++)
        {
            var dismissal = Dismissal.Create(
                WicketType.Bowled,
                null);

            innings.RecordDelivery(
                Delivery(
                    strikerId: innings.CurrentStrikerId,
                    nonStrikerId: innings.CurrentNonStrikerId,
                    dismissal: dismissal,
                    dismissedPlayerId: innings.CurrentStrikerId));
        }

        Assert.Equal(10, innings.Wickets);
        Assert.True(innings.IsCompleted);
    }

    [Fact]
    public void Wicket_ShouldUpdateBowlerWickets()
    {
        var innings = CreateInnings();

        var dismissal = Dismissal.Create(
            WicketType.Bowled,
            null);

        innings.RecordDelivery(
            Delivery(
                dismissal: dismissal,
                dismissedPlayerId: 101));

        var bowlingScore = Assert.Single(innings.BowlingScores);

        Assert.Equal(1, bowlingScore.TotalWickets);
    }

    [Fact]
    public void RunOut_ShouldNotCountAsBowlerWicket()
    {
        var innings = CreateInnings();

        var dismissal = Dismissal.Create(
            WicketType.RunOut,
            301);

        innings.RecordDelivery(
            Delivery(
                dismissal: dismissal,
                dismissedPlayerId: 101));

        var bowlingScore = Assert.Single(innings.BowlingScores);

        Assert.Equal(0, bowlingScore.TotalWickets);
        Assert.Equal(1, innings.Wickets);
    }

    [Fact]
    public void ChangeBowler_ShouldUpdateCurrentBowler()
    {
        var innings = CreateInnings();

        innings.ChangeBowler(202);

        Assert.Equal(202, innings.CurrentBowlerId);
    }

    [Fact]
    public void RecordDelivery_WithWrongStriker_ShouldThrow()
    {
        var innings = CreateInnings();

        Assert.Throws<InvalidOperationException>(() =>
            innings.RecordDelivery(
                Delivery(
                    strikerId: 999,
                    nonStrikerId: 102)));
    }

    [Fact]
    public void RecordDelivery_AfterInningsCompleted_ShouldThrow()
    {
        var innings = CreateInnings(targetRuns: 1);

        innings.RecordDelivery(
            Delivery(batterRuns: 1, totalRuns: 1));

        Assert.True(innings.IsCompleted);

        Assert.Throws<InvalidOperationException>(() =>
            innings.RecordDelivery(
                Delivery()));
    }

    [Fact]
    public void UndoLastDelivery_ShouldRestoreScore()
    {
        var innings = CreateInnings();
        
        // Capture the starting actors dynamically from the active innings state
        int initialStriker = innings.CurrentStrikerId;
        int initialBowler = innings.CurrentBowlerId;

        innings.RecordDelivery(
            Delivery(batterRuns: 4, totalRuns: 4));

        innings.UndoLastDelivery();

        Assert.Equal(0, innings.TotalRuns);
        Assert.Equal(0, innings.TotalBalls);
        
        var battingScore = innings.BattingScores.FirstOrDefault(x => x.PlayerId == initialStriker);
        if (battingScore != null)
        {
            Assert.Equal(0, battingScore.Runs);
            Assert.Equal(0, battingScore.Balls);
        }

        var bowlingScore = innings.BowlingScores.FirstOrDefault(x => x.PlayerId == initialBowler);
        if (bowlingScore != null)
        {
            Assert.Equal(0, bowlingScore.RunsConceded);
            Assert.Equal(0, bowlingScore.Balls);
        }
    }

    [Fact]
    public void UndoLastDelivery_ShouldRestoreStrike()
    {
        var innings = CreateInnings();

        innings.RecordDelivery(
            Delivery(batterRuns: 1,totalRuns: 1));

        Assert.Equal(102, innings.CurrentStrikerId);

        innings.UndoLastDelivery();

        Assert.Equal(101, innings.CurrentStrikerId);
        Assert.Equal(102, innings.CurrentNonStrikerId);
    }

    [Fact]
    public void UndoWicket_ShouldRestoreWicketCount()
    {
        var innings = CreateInnings();

        var dismissal = Dismissal.Create(
            WicketType.Bowled,
            null);

        innings.RecordDelivery(
            Delivery(
                dismissal: dismissal,
                dismissedPlayerId: 101));

        Assert.Equal(1, innings.Wickets);

        innings.UndoLastDelivery();

        Assert.Equal(0, innings.Wickets);
    }

    [Fact]
    public void UndoLastDelivery_AfterCompletedOver_ShouldRestorePreviousOver()
    {
        var innings = CreateInnings();

        for (int i = 0; i < 6; i++)
        {
            innings.RecordDelivery(
                Delivery(
                    strikerId: innings.CurrentStrikerId,
                    nonStrikerId: innings.CurrentNonStrikerId));
        }

        Assert.Equal(2, innings.Overs.Count);

        innings.UndoLastDelivery();

        Assert.Equal(1, innings.Overs.Count);
        Assert.Equal(5, innings.TotalBalls);
    }
}