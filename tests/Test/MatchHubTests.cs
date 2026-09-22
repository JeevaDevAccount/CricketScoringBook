using Microsoft.Extensions.DependencyInjection;
using Application.Matches.Commands.MatchCommandHandlers;
using Application.Matches.Commands.MatchCommands;
using Domain.Aggregates.MatchAggregate;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using tests.Test.Infrastructure;

namespace tests.Test.Integration.SignalR;

public sealed class MatchHubTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MatchHubTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ------------------------------------------------------------
    // 1. Client connection
    // ------------------------------------------------------------

    [Fact]
    public async Task SignalR_Client_Should_Connect_And_Join_Match()
    {
        var matchId = Guid.NewGuid();

        await using var connection = CreateConnection();

        try
        {
            await connection.StartAsync();

            await connection.InvokeAsync(
                "JoinMatch",
                matchId);

            Assert.Equal(
                HubConnectionState.Connected,
                connection.State);
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    // ------------------------------------------------------------
    // 2. Successful delivery -> notification
    // ------------------------------------------------------------

    [Fact]
    public async Task Successful_Delivery_Should_Notify_Client()
    {
        Guid matchId = Guid.Empty;

        try
        {
            var match = await CreateAndPersistStartedMatchAsync();
            matchId = match.Id;

            var innings = match.Innings.Single();

            await using var connection = CreateConnection();

            var received = new TaskCompletionSource<object?>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            connection.On<object>(
                "MatchUpdated",
                match =>
                {
                    received.TrySetResult(match);
                });

            await connection.StartAsync();

            await connection.InvokeAsync(
                "JoinMatch",
                matchId);

            using var scope = _factory.Services.CreateScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<RecordDeliveryHandler>();

            await handler.Handle(
                new RecordDeliveryCommand(
                    matchId,
                    innings.CurrentStrikerId,
                    innings.CurrentNonStrikerId,
                    innings.CurrentBowlerId,
                    BatterRuns: 4,
                    TotalRuns: 4,
                    ExtraType.None,
                    WicketType.None,
                    FielderId: null,
                    DismissedPlayerId: null,
                    IncomingBatterId: null),
                CancellationToken.None);

            var notification = await received.Task
                .WaitAsync(TimeSpan.FromSeconds(5));

            Assert.NotNull(notification);
        }
        finally
        {
            await CleanupMatchAsync(matchId);
        }
    }

    // ------------------------------------------------------------
    // 3. Two clients -> both receive notification
    // ------------------------------------------------------------

    [Fact]
    public async Task Two_Clients_Should_Receive_Same_Match_Update()
    {
        Guid matchId = Guid.Empty;

        try
        {
            var match = await CreateAndPersistStartedMatchAsync();
            matchId = match.Id;

            var innings = match.Innings.Single();

            await using var client1 = CreateConnection();
            await using var client2 = CreateConnection();

            var client1Received =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            var client2Received =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            client1.On<object>(
                "MatchUpdated",
                value => client1Received.TrySetResult(value));

            client2.On<object>(
                "MatchUpdated",
                value => client2Received.TrySetResult(value));

            await client1.StartAsync();
            await client2.StartAsync();

            await client1.InvokeAsync(
                "JoinMatch",
                matchId);

            await client2.InvokeAsync(
                "JoinMatch",
                matchId);

            using var scope = _factory.Services.CreateScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<RecordDeliveryHandler>();

            await handler.Handle(
                new RecordDeliveryCommand(
                    matchId,
                    innings.CurrentStrikerId,
                    innings.CurrentNonStrikerId,
                    innings.CurrentBowlerId,
                    BatterRuns: 4,
                    TotalRuns: 4,
                    ExtraType.None,
                    WicketType.None,
                    FielderId: null,
                    DismissedPlayerId: null,
                    IncomingBatterId: null),
                CancellationToken.None);

            var results = await Task.WhenAll(
                client1Received.Task.WaitAsync(
                    TimeSpan.FromSeconds(5)),
                client2Received.Task.WaitAsync(
                    TimeSpan.FromSeconds(5)));

            Assert.All(results, Assert.NotNull);
        }
        finally
        {
            await CleanupMatchAsync(matchId);
        }
    }

    // ------------------------------------------------------------
    // 4. Match isolation
    // ------------------------------------------------------------

    [Fact]
    public async Task Client_Should_Not_Receive_Update_For_Different_Match()
    {
        Guid matchAId = Guid.Empty;
        Guid matchBId = Guid.Empty;

        try
        {
            var matchA = await CreateAndPersistStartedMatchAsync();
            var matchB = await CreateAndPersistStartedMatchAsync();

            matchAId = matchA.Id;
            matchBId = matchB.Id;

            var inningsB = matchB.Innings.Single();

            await using var clientA = CreateConnection();

            var clientAReceived =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            clientA.On<object>(
                "MatchUpdated",
                value => clientAReceived.TrySetResult(value));

            await clientA.StartAsync();

            await clientA.InvokeAsync(
                "JoinMatch",
                matchAId);

            using var scope = _factory.Services.CreateScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<RecordDeliveryHandler>();

            // Update Match B
            await handler.Handle(
                new RecordDeliveryCommand(
                    matchBId,
                    inningsB.CurrentStrikerId,
                    inningsB.CurrentNonStrikerId,
                    inningsB.CurrentBowlerId,
                    BatterRuns: 4,
                    TotalRuns: 4,
                    ExtraType.None,
                    WicketType.None,
                    FielderId: null,
                    DismissedPlayerId: null,
                    IncomingBatterId: null),
                CancellationToken.None);

            // Give SignalR a short window to deliver an
            // incorrect notification if group isolation is broken.
            var completedTask = await Task.WhenAny(
                clientAReceived.Task,
                Task.Delay(TimeSpan.FromSeconds(2)));

            Assert.NotSame(
                clientAReceived.Task,
                completedTask);
        }
        finally
        {
            await CleanupMatchAsync(matchAId);
            await CleanupMatchAsync(matchBId);
        }
    }

    // ------------------------------------------------------------
    // 5. Failed delivery -> no notification
    // ------------------------------------------------------------

    [Fact]
    public async Task Failed_Delivery_Should_Not_Notify_Client()
    {
        Guid matchId = Guid.Empty;

        try
        {
            var match = await CreateAndPersistStartedMatchAsync();
            matchId = match.Id;

            await using var connection = CreateConnection();

            var received =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            connection.On<object>(
                "MatchUpdated",
                value => received.TrySetResult(value));

            await connection.StartAsync();

            await connection.InvokeAsync(
                "JoinMatch",
                matchId);

            using var scope = _factory.Services.CreateScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<RecordDeliveryHandler>();

            // Non-existent MatchId causes the handler to fail
            // before SaveChanges/notification.
            var invalidMatchId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    new RecordDeliveryCommand(
                        invalidMatchId,
                        101,
                        102,
                        201,
                        BatterRuns: 4,
                        TotalRuns: 4,
                        ExtraType.None,
                        WicketType.None,
                        FielderId: null,
                        DismissedPlayerId: null,
                        IncomingBatterId: null),
                    CancellationToken.None));

            var completedTask = await Task.WhenAny(
                received.Task,
                Task.Delay(TimeSpan.FromSeconds(2)));

            Assert.NotSame(
                received.Task,
                completedTask);
        }
        finally
        {
            await CleanupMatchAsync(matchId);
        }
    }

    // ------------------------------------------------------------
    // 6. Undo -> notification
    // ------------------------------------------------------------

    [Fact]
    public async Task Undo_Delivery_Should_Notify_Client()
    {
        Guid matchId = Guid.Empty;

        try
        {
            var match = await CreateAndPersistStartedMatchAsync();
            matchId = match.Id;

            var innings = match.Innings.Single();

            await using var connection = CreateConnection();

            var deliveryNotification =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            var undoNotification =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            var notificationCount = 0;

            connection.On<object>(
                "MatchUpdated",
                value =>
                {
                    if (Interlocked.Increment(
                            ref notificationCount) == 1)
                    {
                        deliveryNotification.TrySetResult(value);
                    }
                    else
                    {
                        undoNotification.TrySetResult(value);
                    }
                });

            await connection.StartAsync();

            await connection.InvokeAsync(
                "JoinMatch",
                matchId);

            using var scope = _factory.Services.CreateScope();

            var deliveryHandler = scope.ServiceProvider
                .GetRequiredService<RecordDeliveryHandler>();

            await deliveryHandler.Handle(
                new RecordDeliveryCommand(
                    matchId,
                    innings.CurrentStrikerId,
                    innings.CurrentNonStrikerId,
                    innings.CurrentBowlerId,
                    BatterRuns: 4,
                    TotalRuns: 4,
                    ExtraType.None,
                    WicketType.None,
                    FielderId: null,
                    DismissedPlayerId: null,
                    IncomingBatterId: null),
                CancellationToken.None);

            await deliveryNotification.Task
                .WaitAsync(TimeSpan.FromSeconds(5));

            var undoHandler = scope.ServiceProvider
                .GetRequiredService<UndoDeliveryHandler>();

            await undoHandler.Handle(
                new UndoDeliveryCommand(matchId),
                CancellationToken.None);

            var notification = await undoNotification.Task
                .WaitAsync(TimeSpan.FromSeconds(5));

            Assert.NotNull(notification);
            Assert.Equal(2, notificationCount);
        }
        finally
        {
            await CleanupMatchAsync(matchId);
        }
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private HubConnection CreateConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl(
                new Uri(
                    _factory.Server.BaseAddress,
                    "/hubs/match"),
                options =>
                {
                    options.HttpMessageHandlerFactory = _ =>
                        _factory.Server.CreateHandler();
                })
            .Build();
    }

    private async Task<Match> CreateAndPersistStartedMatchAsync()
    {
        var match = CreateStartedMatch();

        await using var context =
            new AppDbContext(CreateDbContextOptions());

        await context.Matches.AddAsync(match);
        await context.SaveChangesAsync();

        return match;
    }

    private static Match CreateStartedMatch()
    {
        int team1Id =
            Random.Shared.Next(100000, 200000);

        int team2Id =
            Random.Shared.Next(200001, 300000);

        var match = Match.Create(
            team1Id,
            team2Id,
            maxOvers: 10);

        int strikerId =
            Random.Shared.Next(300001, 400000);

        int nonStrikerId =
            Random.Shared.Next(400001, 500000);

        int bowlerId =
            Random.Shared.Next(500001, 600000);

        int extraPlayerId =
            Random.Shared.Next(600001, 700000);

        match.AddPlayerToPlayingTeam(
            team1Id,
            strikerId);

        match.AddPlayerToPlayingTeam(
            team1Id,
            nonStrikerId);

        match.AddPlayerToPlayingTeam(
            team2Id,
            bowlerId);

        match.AddPlayerToPlayingTeam(
            team2Id,
            extraPlayerId);

        match.ClaimScorer(999);

        match.Toss(
            tossWonTeamId: team1Id,
            decision: InningsDecision.Bat);

        match.StartMatch();

        match.StartInnings(
            strikerId,
            nonStrikerId,
            bowlerId);

        return match;
    }

    private static DbContextOptions<AppDbContext>
        CreateDbContextOptions()
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=CricketScoring;Username=postgres;Password=postgres";

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    private static async Task CleanupMatchAsync(
        Guid matchId)
    {
        if (matchId == Guid.Empty)
            return;

        await using var context =
            new AppDbContext(CreateDbContextOptions());

        var match = await context.Matches
            .SingleOrDefaultAsync(x => x.Id == matchId);

        if (match is null)
            return;

        context.Matches.Remove(match);

        await context.SaveChangesAsync();
    }
}