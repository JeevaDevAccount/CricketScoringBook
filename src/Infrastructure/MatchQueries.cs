using Application.Abstractions.Interfaces;
using Application.Matches.Queries;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

public sealed class MatchQueries : IMatchQueries
{
    private readonly AppDbContext _context;

    public MatchQueries(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MatchDto?> GetMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        return await _context.Matches
            .AsNoTracking()
            .Where(x => x.Id == matchId)
            .Select(x => new MatchDto(
                x.Id,

                x.Team1PlayingTeam.TeamId,
                _context.Set<Team>()
                    .Where(t => t.Id == x.Team1PlayingTeam.TeamId)
                    .Select(t => t.Name)
                    .First(),

                x.Team2PlayingTeam.TeamId,
                _context.Set<Team>()
                    .Where(t => t.Id == x.Team2PlayingTeam.TeamId)
                    .Select(t => t.Name)
                    .First(),

                x.Status,
                x.TeamBattingFirstId,
                x.TeamBattingSecondId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<LiveMatchDto?> GetLiveMatchAsync(Guid matchId, CancellationToken cancellationToken)
    {
        // Fetch the raw query object target
        var matchData = await _context.Matches
            .AsNoTracking()
            .Where(x => x.Id == matchId)
            .Select(x => new
            {
                x.Id,
                x.Status,
                // Extract the last active innings row safely using LastOrDefault
                LastInnings = x.Innings.OrderBy(i => i.InningsNumber).LastOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (matchData is null)
            return null;

        var innings = matchData.LastInnings;

        if (innings is null)
            return null;
        
        var battingTeam = await _context.Set<Team>()
        .AsNoTracking()
        .Where(t => t.Id == innings.BattingTeamId)
        .Select(t => new { t.Id, t.Name })
        .SingleAsync(cancellationToken);

    var bowlingTeam = await _context.Set<Team>()
        .AsNoTracking()
        .Where(t => t.Id == innings.BowlingTeamId)
        .Select(t => new { t.Id, t.Name })
        .SingleAsync(cancellationToken);

    var playerIds = new[]
    {
        innings.CurrentStrikerId,
        innings.CurrentNonStrikerId,
        innings.CurrentBowlerId
    };

    var players = await _context.Set<Player>()
        .AsNoTracking()
        .Where(p => playerIds.Contains(p.Id))
        .Select(p => new
        {
            p.Id,
            p.Name
        })
        .ToDictionaryAsync(
            p => p.Id,
            cancellationToken);

    return new LiveMatchDto(
        matchData.Id,
        matchData.Status,

        battingTeam.Id,
        battingTeam.Name,

        bowlingTeam.Id,
        bowlingTeam.Name,

        innings.TotalRuns,
        innings.TargetRuns,
        innings.Wickets,
        innings.TotalBalls,

        innings.CurrentStrikerId,
        players[innings.CurrentStrikerId].Name,

        innings.CurrentNonStrikerId,
        players[innings.CurrentNonStrikerId].Name,

        innings.CurrentBowlerId,
        players[innings.CurrentBowlerId].Name,

        innings.Overs
            .OrderBy(o => o.OverNumber)
            .LastOrDefault()?.OverNumber ?? 0);
    }


    public async Task<ScorecardDto?> GetScorecardAsync(Guid matchId,CancellationToken cancellationToken)
    {
        return await _context.Matches
            .AsNoTracking()
            .Where(x => x.Id == matchId)
            .Select(x => new ScorecardDto(
                x.Id,

                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Select(i => new InningsScorecardDto(
                        i.InningsNumber,

                        i.BattingTeamId,
                        _context.Set<Team>()
                            .Where(t => t.Id == i.BattingTeamId)
                            .Select(t => t.Name)
                            .First(),

                        i.BowlingTeamId,
                        _context.Set<Team>()
                            .Where(t => t.Id == i.BowlingTeamId)
                            .Select(t => t.Name)
                            .First(),

                        i.TotalRuns,
                        i.Wickets,
                        i.TotalBalls,

                        i.BattingScores
                            .Select(b => new BattingScoreDto(
                                b.PlayerId,
                                _context.Set<Player>()
                                    .Where(p => p.Id == b.PlayerId)
                                    .Select(p => p.Name)
                                    .First(),

                                b.Runs,
                                b.Balls,
                                b.Fours,
                                b.Sixes,
                                b.StrikeRate,
                                b.Dismissal))
                            .ToList(),

                        i.BowlingScores
                            .Select(b => new BowlingScoreDto(
                                b.PlayerId,
                                _context.Set<Player>()
                                    .Where(p => p.Id == b.PlayerId)
                                    .Select(p => p.Name)
                                    .First(),

                                b.RunsConceded,
                                b.Balls,
                                b.Maidens,
                                b.TotalWickets,
                                b.Economy))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}