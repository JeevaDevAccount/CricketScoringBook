using Application.Abstractions.Interfaces;
using Application.Matches.Commands.MatchCommandHandlers;
using Application.Matches.Commands.MatchCommands;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Endpoints;

public static class MatchesEndpoints
{
    public static void MapMatchesEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/matches")
            .WithTags("Cricket Matches");

        // 1. Claim scorer (Binds Command directly from Body, patches MatchId from URL)
        group.MapPost(
            "/{matchId:guid}/scorer/claim",
            async (
                Guid matchId,
                [FromBody] ClaimScorerCommand command,
                ClaimScorerHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    command with { MatchId = matchId },
                    cancellationToken);

                return Results.NoContent();
            });

        // 2. Change scorer
        group.MapPost(
            "/{matchId:guid}/scorer/change",
            async (
                Guid matchId,
                [FromBody] ChangeScorerCommand command,
                ChangeScorerHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    command with { MatchId = matchId },
                    cancellationToken);

                return Results.NoContent();
            });

        // 3. Record toss
        group.MapPost(
            "/{matchId:guid}/toss",
            async (
                Guid matchId,
                [FromBody] RecordTossCommand command,
                RecordTossHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    command with { MatchId = matchId },
                    cancellationToken);

                return Results.NoContent();
            });

        // 4. Start match (No Body needed, instantiates using purely the URL Guid)
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

        // 5. Start innings
        group.MapPost(
            "/{matchId:guid}/innings/start",
            async (
                Guid matchId,
                [FromBody] StartInningsCommand command,
                StartInningsHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    command with { MatchId = matchId },
                    cancellationToken);

                return Results.NoContent();
            });

        // 6. Record delivery
        group.MapPost(
            "/{matchId:guid}/deliveries",
            async (
                Guid matchId,
                [FromBody] RecordDeliveryCommand command,
                RecordDeliveryHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    command with { MatchId = matchId },
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

        // 8. Add player to playing XI
        group.MapPost(
            "/{matchId:guid}/teams/{teamId:int}/players",
            async (
                Guid matchId,
                int teamId,
                [FromBody] AddPlayerToPlayingTeamCommand command,
                AddPlayerToPlayingTeamHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(
                    command with { MatchId = matchId, TeamId = teamId },
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
    }
}
