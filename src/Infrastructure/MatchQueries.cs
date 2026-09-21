using Application.Abstractions.Interfaces;
using Application.Matches.Queries;
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
                x.Team2PlayingTeam.TeamId,
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

        int battingTeamId = matchData.LastInnings?.BattingTeamId ?? 0;
        int bowlingTeamId = matchData.LastInnings?.BowlingTeamId ?? 0;
        int totalRuns = matchData.LastInnings?.TotalRuns ?? 0;
        int? targetRuns = matchData.LastInnings?.TargetRuns;
        int wickets = matchData.LastInnings?.Wickets ?? 0;
        int totalBalls = matchData.LastInnings?.TotalBalls ?? 0;
        int currentStrikerId = matchData.LastInnings?.CurrentStrikerId ?? 0;
        int currentNonStrikerId = matchData.LastInnings?.CurrentNonStrikerId ?? 0;
        int currentBowlerId = matchData.LastInnings?.CurrentBowlerId ?? 0;

        // Pull down the active current over index safely without indexing crashes
        int currentOverNumber = matchData.LastInnings?.Overs
            .OrderBy(o => o.OverNumber)
            .LastOrDefault()?.OverNumber ?? 0;

        return new LiveMatchDto(
            matchData.Id,
            matchData.Status,
            battingTeamId,
            bowlingTeamId,
            totalRuns,
            targetRuns,
            wickets,
            totalBalls,
            currentStrikerId,
            currentNonStrikerId,
            currentOverNumber,
            currentBowlerId
        );
    }


    public async Task<ScorecardDto?> GetScorecardAsync(
        Guid matchId,
        CancellationToken cancellationToken)
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
                        i.BowlingTeamId,
                        i.TotalRuns,
                        i.Wickets,
                        i.TotalBalls,

                        i.BattingScores
                            .Select(b => new BattingScoreDto(
                                b.PlayerId,
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