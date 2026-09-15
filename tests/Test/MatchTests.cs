using Domain.Aggregates.MatchAggregate;
using Domain.Enum;

namespace tests.Test;

public sealed class MatchTests
{
    private static Match CreateMatch()
    {
        return Match.Create(
            team1Id: 1,
            team2Id: 2,
            maxOvers: 10);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        var match1 = CreateMatch();
        var match2 = CreateMatch();

        Assert.NotEqual(match1.Id, match2.Id);
    }

    [Fact]
    public void Create_ShouldHaveNoActiveScorer()
    {
        var match = CreateMatch();

        Assert.Null(match.ActiveScorerId);
    }

    [Fact]
    public void ClaimScorer_ShouldAssignScorer()
    {
        var match = CreateMatch();

        match.ClaimScorer(101);

        Assert.Equal(101, match.ActiveScorerId);
    }

    [Fact]
    public void SecondScorer_ShouldNotClaimAlreadyClaimedMatch()
    {
        var match = CreateMatch();

        match.ClaimScorer(101);

        Assert.Throws<InvalidOperationException>(() =>
            match.ClaimScorer(202));
    }

    [Fact]
    public void ChangeScorer_ShouldReplaceCurrentScorer()
    {
        var match = CreateMatch();

        match.ClaimScorer(101);
        match.ChangeScorer(202);

        Assert.Equal(202, match.ActiveScorerId);
    }

    [Fact]
    public void InvalidTeamId_ShouldNotCreateMatch()
    {
        Assert.Throws<ArgumentException>(() =>
            Match.Create(
                team1Id: 0,
                team2Id: 2,
                maxOvers: 10));
    }
}