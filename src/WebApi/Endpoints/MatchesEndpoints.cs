using Application.Abstractions.Interfaces;
using Application.Matches.Commands.MatchCommandHandlers;
using Application.Matches.Commands.MatchCommands;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Endpoints;

public static class MatchesEndpoints
{
    public static void MapMatchesEndpoints(this IEndpointRouteBuilder app)
    {
        // ============================================================
        // PLAYER ENDPOINTS
        // ============================================================

        var playerGroup = app
            .MapGroup("/api/players")
            .WithTags("Players");

        // Create a new master player
        playerGroup.MapPost(
            "",
            async (
                [FromBody] CreatePlayerCommand command,
                [FromServices] CreatePlayerHandler handler,
                CancellationToken cancellationToken) =>
            {
                var playerId = await handler.Handle(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/players/{playerId}",
                    new { Id = playerId });
            });

        // ============================================================
        // Search existing players
        // ============================================================

        playerGroup.MapGet(
            "/search",
            async (
                string name,
                [FromServices] IPlayerQueries queries,
                CancellationToken cancellationToken) =>
            {
                var players = await queries.SearchPlayersAsync(
                    name,
                    cancellationToken);

                return Results.Ok(players);
            }
        );

        // ============================================================
        // MATCH ENDPOINTS
        // ============================================================

        var group = app
            .MapGroup("/api/matches")
            .WithTags("Cricket Matches");

        // 0. Create Match
        group.MapPost(
            "/",
            async (
                [FromBody] CreateMatchCommand command,
                CreateMatchHandler handler,
                CancellationToken cancellationToken) =>
            {
                Guid matchId = await handler.Handle(
                    command,
                    cancellationToken);

                return Results.CreatedAtRoute(
                    "GetLiveMatch",
                    new { matchId },
                    new { Id = matchId });
            });

        // 1. Start match
        group.MapPost(
            "/{matchId:guid}/start",
            async (
                Guid matchId,
                StartMatchHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    new StartMatchCommand(matchId),
                    cancellationToken);

                return Results.NoContent();
            });

        // 2. Claim scorer
        group.MapPost(
            "/{matchId:guid}/scorer/claim",
            async (
                Guid matchId,
                [FromBody] ClaimScorerBody body,
                ClaimScorerHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    new ClaimScorerCommand(
                        matchId,
                        body.ScorerId),
                    cancellationToken);

                return Results.NoContent();
            });

        // 3. Change scorer
        group.MapPost(
            "/{matchId:guid}/scorer/change",
            async (
                Guid matchId,
                [FromBody] ChangeScorerBody body,
                ChangeScorerHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    new ChangeScorerCommand(
                        matchId,
                        body.ScorerId),
                    cancellationToken);

                return Results.NoContent();
            });

        // 4. Record toss
        group.MapPost(
            "/{matchId:guid}/toss",
            async (
                Guid matchId,
                [FromBody] RecordTossBody body,
                RecordTossHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RecordTossCommand(
                    matchId,
                    body.TossWonTeamId,
                    body.Decision ?? InningsDecision.Bat);

                await handler.Handle(
                    command,
                    cancellationToken);

                return Results.NoContent();
            });

        // 5. Start innings
        group.MapPost(
            "/{matchId:guid}/innings/start",
            async (
                Guid matchId,
                [FromBody] StartInningsBody body,
                StartInningsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new StartInningsCommand(
                    matchId,
                    body.StrikerId,
                    body.NonStrikerId,
                    body.BowlerId);

                await handler.Handle(
                    command,
                    cancellationToken);

                return Results.NoContent();
            });

        // 6. Record delivery
        group.MapPost(
            "/{matchId:guid}/deliveries",
            async (
                Guid matchId,
                [FromBody] RecordDeliveryBody body,
                RecordDeliveryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RecordDeliveryCommand(
                    matchId,
                    body.StrikerId,
                    body.NonStrikerId,
                    body.BowlerId,
                    body.BatterRuns,
                    body.TotalRuns,
                    body.ExtraType,
                    body.WicketType,
                    body.FielderId,
                    body.DismissedPlayerId,
                    body.IncomingBatterId);

                await handler.Handle(
                    command,
                    cancellationToken);

                return Results.NoContent();
            });

        // 7. Undo delivery
        group.MapPost(
            "/{matchId:guid}/deliveries/undo",
            async (
                Guid matchId,
                UndoDeliveryHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    new UndoDeliveryCommand(matchId),
                    cancellationToken);

                return Results.NoContent();
            });

        // 8. Add existing player to playing XI
        group.MapPost(
            "/{matchId:guid}/teams/{teamId:int}/players",
            async (
                Guid matchId,
                int teamId,
                [FromBody] AddPlayerBody body,
                AddPlayerToPlayingTeamHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    new AddPlayerToPlayingTeamCommand(
                        matchId,
                        teamId,
                        body.PlayerId),
                    cancellationToken);

                return Results.NoContent();
            });

        // 9. Get match
        group.MapGet(
            "/{matchId:guid}",
            async (
                Guid matchId,
                IMatchQueries queries,
                CancellationToken cancellationToken) =>
            {
                var result = await queries.GetMatchAsync(
                    matchId,
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            });

        // 10. Get live match
        group.MapGet(
            "/{matchId:guid}/live",
            async (
                Guid matchId,
                IMatchQueries queries,
                CancellationToken cancellationToken) =>
            {
                var result = await queries.GetLiveMatchAsync(
                    matchId,
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            })
            .WithName("GetLiveMatch");

        // 11. Get scorecard
        group.MapGet(
            "/{matchId:guid}/scorecard",
            async (
                Guid matchId,
                IMatchQueries queries,
                CancellationToken cancellationToken) =>
            {
                var result = await queries.GetScorecardAsync(
                    matchId,
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            });
        
        // 12. Get all matches (For Match List Page)
        group.MapGet(
            "/getAllMatches",
            async (
                IMatchQueries queries,
                CancellationToken cancellationToken) =>
            {
                var result = await queries.GetAllMatchesAsync(cancellationToken);
                return Results.Ok(result);
            });
    }
}

public sealed record ClaimScorerBody(int ScorerId);

public sealed record ChangeScorerBody(int ScorerId);

public sealed class RecordTossBody
{
    public int TossWonTeamId { get; set; }
    public InningsDecision? Decision { get; set; }
}

public sealed record AddPlayerBody(int PlayerId);

public sealed class StartInningsBody
{
    public int StrikerId { get; set; }
    public int NonStrikerId { get; set; }
    public int BowlerId { get; set; }
}

public sealed class RecordDeliveryBody
{
    public int StrikerId { get; set; }
    public int NonStrikerId { get; set; }
    public int BowlerId { get; set; }
    public int BatterRuns { get; set; }
    public int TotalRuns { get; set; }
    public ExtraType ExtraType { get; set; }
    public WicketType WicketType { get; set; }
    public int? FielderId { get; set; }
    public int? DismissedPlayerId { get; set; }
    public int? IncomingBatterId { get; set; }
}