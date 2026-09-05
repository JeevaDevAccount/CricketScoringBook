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

    public async Task<LiveMatchDto?> GetLiveMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        return await _context.Matches
            .AsNoTracking()
            .Where(x => x.Id == matchId)
            .Select(x => new LiveMatchDto(
                x.Id,
                x.Status,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().BattingTeamId,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().BowlingTeamId,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().TotalRuns,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().TargetRuns,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().Wickets,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().TotalBalls,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().CurrentStrikerId,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().CurrentNonStrikerId,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().CurrentBowlerId,
                x.Innings
                    .OrderBy(i => i.InningsNumber)
                    .Last().Overs
                    .OrderBy(o => o.OverNumber)
                    .Last().OverNumber))
            .SingleOrDefaultAsync(cancellationToken);
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