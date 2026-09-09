using Domain.Aggregates.MatchAggregate;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace tests.Test;

[Collection("DatabaseTests")]
public sealed class MatchPersistenceTests
{
private const string ConnectionString =
"Host=localhost;Port=5432;Database=CricketScoring;Username=postgres;Password=postgres";

// ------------------------------------------------------------
// 1. Match + Playing Teams + Players
// ------------------------------------------------------------

[Fact]
public async Task Match_WithPlayingTeamsAndPlayers_PersistsAndReloadsCorrectly()
{
    var options = CreateDbContextOptions();
    Guid matchId = Guid.Empty;

    try
    {
        // Arrange
        int team1Id = Random.Shared.Next(100000, 200000);
        int team2Id = Random.Shared.Next(200001, 300000);

        var match = Match.Create(
            team1Id,
            team2Id,
            maxOvers: 10);

        match.AddPlayerToPlayingTeam(team1Id, 101);
        match.AddPlayerToPlayingTeam(team1Id, 102);
        match.AddPlayerToPlayingTeam(team1Id, 103);

        match.AddPlayerToPlayingTeam(team2Id, 201);
        match.AddPlayerToPlayingTeam(team2Id, 202);
        match.AddPlayerToPlayingTeam(team2Id, 203);

        matchId = match.Id;

        // Act
        await SaveMatchAsync(options, match);

        // Assert
        await using var context = new AppDbContext(options);

        // Eager load the entire aggregate tree using our fresh clean split tables
        var savedMatch = await context.Matches
            .Include(x => x.Team1PlayingTeam)
                .ThenInclude(x => x.Players)
            .Include(x => x.Team2PlayingTeam)
                .ThenInclude(x => x.Players)
            .SingleOrDefaultAsync(x => x.Id == matchId);

        Assert.NotNull(savedMatch);

        // 🌟 THE PERMANENT FIX: Assert strictly on the specific table split graph properties
        Assert.Equal(3, savedMatch.Team1PlayingTeam.Players.Count);
        Assert.Equal(3, savedMatch.Team2PlayingTeam.Players.Count);

        // Verify Team 1 Players exist in the correct property collection
        var team1Players = savedMatch.Team1PlayingTeam.Players.Select(p => p.PlayerId).ToList();
        Assert.Contains(101, team1Players);
        Assert.Contains(102, team1Players);
        Assert.Contains(103, team1Players);

        // Verify Team 2 Players exist in the correct property collection
        var team2Players = savedMatch.Team2PlayingTeam.Players.Select(p => p.PlayerId).ToList();
        Assert.Contains(201, team2Players);
        Assert.Contains(202, team2Players);
        Assert.Contains(203, team2Players);
    }
    finally
    {
        await CleanupMatchAsync(options, matchId);
    }
}

// ------------------------------------------------------------
// 2. Match + Innings + Over
// ------------------------------------------------------------

[Fact]
public async Task Match_WithInningsAndOver_PersistsAndReloadsCorrectly()
{
    var options = CreateDbContextOptions();

    Guid matchId = Guid.Empty;

    try
    {
        // Arrange
        var match = CreateStartedMatch();

        matchId = match.Id;

        // Act
        await SaveMatchAsync(options, match);

        // Assert
        await using var context = new AppDbContext(options);

        var savedMatch = await context.Matches
            .Include(x => x.Innings)
                .ThenInclude(x => x.Overs)
            .SingleOrDefaultAsync(x => x.Id == matchId);

        Assert.NotNull(savedMatch);

        Assert.Single(savedMatch.Innings);

        var innings = savedMatch.Innings.Single();

        Assert.Equal(1, innings.InningsNumber);
        Assert.Equal(
            savedMatch.Team1PlayingTeam.TeamId,
            innings.BattingTeamId);

        Assert.Equal(
            savedMatch.Team2PlayingTeam.TeamId,
            innings.BowlingTeamId);

        Assert.Equal(InningsType.Regular, innings.Type);
        Assert.Equal(10, innings.MaxOvers);

        Assert.Single(innings.Overs);

        var over = innings.Overs.Single();

        Assert.Equal(1, over.OverNumber);
        Assert.False(over.IsCompleted);
        Assert.True(over.IsEditable);
        Assert.Empty(over.Deliveries);
    }
    finally
    {
        await CleanupMatchAsync(options, matchId);
    }
}

// ------------------------------------------------------------
// 3. Match + Innings + Over + Delivery
// ------------------------------------------------------------

[Fact]
public async Task Match_WithDelivery_PersistsAndReloadsCorrectly()
{
    var options = CreateDbContextOptions();
    Guid matchId = Guid.Empty;

    try
    {
        // Arrange
        var match = CreateStartedMatch();
        matchId = match.Id;

        // 🌟 THE FIX: Pull the active player IDs straight from the initialized domain graph
        var innings = match.Innings.Single();
        int strikerId = innings.CurrentStrikerId;
        int nonStrikerId = innings.CurrentNonStrikerId;
        int bowlerId = innings.CurrentBowlerId;

        var delivery = new Match.DeliveryInput(
            StrikerId: strikerId,
            NonStrikerId: nonStrikerId,
            BowlerId: bowlerId,
            BatterRuns: 4,
            TotalRuns: 4,
            ExtraType: ExtraType.None,
            Dismissal: null!,
            DismissedPlayerId: null);

        match.RecordDelivery(delivery);

        // Act
        await SaveMatchAsync(options, match);

        // Assert
        await using var context = new AppDbContext(options);
        var savedMatch = await context.Matches
            .Include(x => x.Innings)
                .ThenInclude(x => x.Overs)
                    .ThenInclude(x => x.Deliveries)
            .SingleOrDefaultAsync(x => x.Id == matchId);

        Assert.NotNull(savedMatch);
        var savedInnings = savedMatch.Innings.Single();
        var over = savedInnings.Overs.Single();
        var savedDelivery = over.Deliveries.Single();

        Assert.Equal(1, savedDelivery.SequenceNumber);
        Assert.Equal(1, savedDelivery.BallNumberInOver);
        Assert.Equal(strikerId, savedDelivery.StrikerId);
        Assert.Equal(nonStrikerId, savedDelivery.NonStrikerId);
        Assert.Equal(bowlerId, savedDelivery.BowlerId);
        Assert.Equal(4, savedDelivery.BatterRuns);
        Assert.Equal(4, savedDelivery.TotalRuns);
    }
    finally
    {
        await CleanupMatchAsync(options, matchId);
    }
}


// ------------------------------------------------------------
// 4. Match + BattingScore + BowlingScore
// ------------------------------------------------------------

[Fact]
public async Task Match_WithBattingAndBowlingScores_PersistsAndReloadsCorrectly()
{
    var options = CreateDbContextOptions();
    Guid matchId = Guid.Empty;

    try
    {
        // Arrange
        var match = CreateStartedMatch();
        matchId = match.Id;

        // 🌟 THE FIX: Dynamically track players from the active innings state
        var innings = match.Innings.Single();
        int strikerId = innings.CurrentStrikerId;
        int bowlerId = innings.CurrentBowlerId;

        var delivery = new Match.DeliveryInput(
            StrikerId: strikerId,
            NonStrikerId: innings.CurrentNonStrikerId,
            BowlerId: bowlerId,
            BatterRuns: 4,
            TotalRuns: 4,
            ExtraType: ExtraType.None,
            Dismissal: null!,
            DismissedPlayerId: null);

        match.RecordDelivery(delivery);

        // Act
        await SaveMatchAsync(options, match);

        // Assert
        await using var context = new AppDbContext(options);
        var savedMatch = await context.Matches
            .Include(x => x.Innings)
            .SingleOrDefaultAsync(x => x.Id == matchId);

        Assert.NotNull(savedMatch);

        // Fetch scoreboards via explicit context queries since they are independent roots
        var battingScore = await context.Set<BattingScore>()
            .SingleOrDefaultAsync(x => EF.Property<Guid>(x, "InningsId") == innings.Id && x.PlayerId == strikerId);
            
        var bowlingScore = await context.Set<BowlingScore>()
            .SingleOrDefaultAsync(x => EF.Property<Guid>(x, "InningsId") == innings.Id && x.PlayerId == bowlerId);

        Assert.NotNull(battingScore);
        Assert.NotNull(bowlingScore);

        Assert.Equal(strikerId, battingScore.PlayerId);
        Assert.Equal(4, battingScore.Runs);
        Assert.Equal(1, battingScore.Balls);

        Assert.Equal(bowlerId, bowlingScore.PlayerId);
        Assert.Equal(4, bowlingScore.RunsConceded);
    }
    finally
    {
        await CleanupMatchAsync(options, matchId);
    }
}

// ------------------------------------------------------------
// 5. Complete aggregate round-trip
// ------------------------------------------------------------

[Fact]
public async Task CompleteMatchAggregate_PersistsAndReloadsCorrectly()
{
    var options = CreateDbContextOptions();

    Guid matchId = Guid.Empty;

    try
    {
        // Arrange
        var match = CreateStartedMatch();

        matchId = match.Id;

        // Playing XI additions (Appends a 3rd player to each team setup)
        match.AddPlayerToPlayingTeam(
            match.Team1PlayingTeam.TeamId,
            103);

        match.AddPlayerToPlayingTeam(
            match.Team2PlayingTeam.TeamId,
            202);

        // Delivery 1 - 4 runs
        match.RecordDelivery(
            new Match.DeliveryInput(
                StrikerId: 101,
                NonStrikerId: 102,
                BowlerId: 201,
                BatterRuns: 4,
                TotalRuns: 4,
                ExtraType: ExtraType.None,
                Dismissal: null!,
                DismissedPlayerId: null));

        // Delivery 2 - 1 run
        match.RecordDelivery(
            new Match.DeliveryInput(
                StrikerId: 101,
                NonStrikerId: 102,
                BowlerId: 201,
                BatterRuns: 1,
                TotalRuns: 1,
                ExtraType: ExtraType.None,
                Dismissal: null!,
                DismissedPlayerId: null));

        // Delivery 3 - dot ball
        match.RecordDelivery(
            new Match.DeliveryInput(
                StrikerId: 102,
                NonStrikerId: 101,
                BowlerId: 201,
                BatterRuns: 0,
                TotalRuns: 0,
                ExtraType: ExtraType.None,
                Dismissal: null!,
                DismissedPlayerId: null));

        // Act
        await SaveMatchAsync(options, match);

        // Assert
        await using var context = new AppDbContext(options);

        // 🌟 Step 1: Eager load core aggregate hierarchy cleanly without relational loop explosions
        var savedMatch = await context.Matches
            .Include(x => x.Team1PlayingTeam)
                .ThenInclude(x => x.Players)
            .Include(x => x.Team2PlayingTeam)
                .ThenInclude(x => x.Players)
            .Include(x => x.Innings)
                .ThenInclude(x => x.Overs)
                    .ThenInclude(x => x.Deliveries)
            .SingleOrDefaultAsync(x => x.Id == matchId);

        Assert.NotNull(savedMatch);

        var innings = savedMatch.Innings.Single();

        // 🌟 Step 2: Fetch the scorecard roots directly from their standalone tracking sets
        var battingScores = await context.Set<BattingScore>()
            .Where(x => EF.Property<Guid>(x, "InningsId") == innings.Id)
            .ToListAsync();

        var bowlingScores = await context.Set<BowlingScore>()
            .Where(x => EF.Property<Guid>(x, "InningsId") == innings.Id)
            .ToListAsync();

        // Match Core Validation
        Assert.Equal(matchId, savedMatch.Id);
        Assert.Equal(MatchStatus.Live, savedMatch.Status);
        Assert.Equal(10, savedMatch.MaxOvers);

        // Playing teams (Expected 3 due to 2 initial players + 1 added above)
        Assert.Equal(3, savedMatch.Team1PlayingTeam.Players.Count);
        Assert.Equal(3, savedMatch.Team2PlayingTeam.Players.Count);

        Assert.Contains(
            savedMatch.Team1PlayingTeam.Players,
            x => x.PlayerId == 103);

        Assert.Contains(
            savedMatch.Team2PlayingTeam.Players,
            x => x.PlayerId == 202);

        // Innings Calculations
        Assert.Equal(1, innings.InningsNumber);
        Assert.Equal(RegularInningsBattingTeam(savedMatch), innings.BattingTeamId);
        Assert.Equal(5, innings.TotalRuns); // Cumulative runs math fix: 4 + 1 + 0 = 5
        Assert.Equal(3, innings.TotalBalls);
        Assert.Equal(0, innings.Wickets);
        Assert.False(innings.IsCompleted);

        // Over Validation
        Assert.Single(innings.Overs);

        var over = innings.Overs.Single();

        Assert.Equal(1, over.OverNumber);
        Assert.False(over.IsCompleted);
        Assert.True(over.IsEditable);

        // Deliveries Details
        Assert.Equal(3, over.Deliveries.Count);

        var deliveries = over.Deliveries
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        Assert.Equal(1, deliveries[0].SequenceNumber);
        Assert.Equal(1, deliveries[0].BallNumberInOver);
        Assert.Equal(4, deliveries[0].BatterRuns);
        Assert.Equal(4, deliveries[0].TotalRuns);

        Assert.Equal(2, deliveries[1].SequenceNumber);
        Assert.Equal(2, deliveries[1].BallNumberInOver);
        Assert.Equal(1, deliveries[1].BatterRuns);
        Assert.Equal(1, deliveries[1].TotalRuns);

        Assert.Equal(3, deliveries[2].SequenceNumber);
        Assert.Equal(3, deliveries[2].BallNumberInOver);
        Assert.Equal(0, deliveries[2].BatterRuns);
        Assert.Equal(0, deliveries[2].TotalRuns);

        // Batting score card evaluations against explicitly loaded list
        Assert.Equal(2, battingScores.Count);

        var strikerScore = battingScores
            .Single(x => x.PlayerId == 101);

        Assert.Equal(5, strikerScore.Runs);
        Assert.Equal(2, strikerScore.Balls);
        Assert.Equal(1, strikerScore.Fours);
        Assert.Equal(0, strikerScore.Sixes);
        Assert.Equal(250m, strikerScore.StrikeRate);
        Assert.Null(strikerScore.Dismissal);

        var nonStrikerScore = battingScores
            .Single(x => x.PlayerId == 102);

        Assert.Equal(0, nonStrikerScore.Runs);
        Assert.Equal(1, nonStrikerScore.Balls);
        Assert.Equal(0, nonStrikerScore.Fours);
        Assert.Equal(0, nonStrikerScore.Sixes);
        Assert.Equal(0m, nonStrikerScore.StrikeRate);

        // Bowling score card evaluations against explicitly loaded list
        Assert.Single(bowlingScores);

        var bowlingScore = bowlingScores.Single();

        Assert.Equal(201, bowlingScore.PlayerId);
        Assert.Equal(5, bowlingScore.RunsConceded); // Conceded matches cumulative innings total runs (5)
        Assert.Equal(3, bowlingScore.Balls);
        Assert.Equal(0, bowlingScore.Maidens);
        Assert.Equal(0, bowlingScore.TotalWickets);
        Assert.Equal(10m, bowlingScore.Economy); // 5 runs conceded / 0.5 overs bowled = 10.00 economy rate
    }
    finally
    {
        await CleanupMatchAsync(options, matchId);
    }
}

// ------------------------------------------------------------
// Helpers
// ------------------------------------------------------------

private static DbContextOptions<AppDbContext> CreateDbContextOptions()
{
    return new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(ConnectionString)
        .Options;
}

private static async Task SaveMatchAsync(DbContextOptions<AppDbContext> options,Match match)
{
    await using var context = new AppDbContext(options);
    await context.Matches.AddAsync(match);
    //context.Matches.Add(match);
    await context.SaveChangesAsync();
}

private static async Task CleanupMatchAsync(
    DbContextOptions<AppDbContext> options,
    Guid matchId)
{
    if (matchId == Guid.Empty)
        return;

    await using var context = new AppDbContext(options);

    var match = await context.Matches
        .SingleOrDefaultAsync(x => x.Id == matchId);

    if (match is null)
        return;

    context.Matches.Remove(match);

    await context.SaveChangesAsync();
}

private static Match CreateStartedMatch()
{
    int team1Id = Random.Shared.Next(100000, 200000);
    int team2Id = Random.Shared.Next(200001, 300000);

    var match = Match.Create(
        team1Id,
        team2Id,
        maxOvers: 10);

    // Generate unique player IDs for this specific execution
    int strikerId = Random.Shared.Next(300001, 400000);
    int nonStrikerId = Random.Shared.Next(400001, 500000);
    int bowlerId = Random.Shared.Next(500001, 600000);
    int extraPlayerId = Random.Shared.Next(600001, 700000);

    match.AddPlayerToPlayingTeam(team1Id, strikerId);
    match.AddPlayerToPlayingTeam(team1Id, nonStrikerId);

    match.AddPlayerToPlayingTeam(team2Id, bowlerId);
    match.AddPlayerToPlayingTeam(team2Id, extraPlayerId);

    // Claim scorer.
    match.ClaimScorer(999);

    // Team 1 wins toss and bats.
    match.Toss(
        tossWonTeamId: team1Id,
        decision: InningsDecision.Bat);

    // Start match.
    match.StartMatch();

    // Start first innings.
    match.StartInnings(
        strikerId: 101,
        nonStrikerId: 102,
        bowlerId: 201);

    return match;
}

private static int RegularInningsBattingTeam(Match match)
{
    return match.Team1PlayingTeam.TeamId;
}
}
