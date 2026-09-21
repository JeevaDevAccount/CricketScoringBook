using Application.Abstractions.Interfaces;
using Domain.Aggregates.MatchAggregate;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _context;

    public MatchRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Match match)
    {
        _context.Matches.Add(match);
    }
    public async Task<Match?> GetByIdAsync(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        return await _context.Matches
            .Include(x => x.Team1PlayingTeam)
            .Include(x => x.Team2PlayingTeam)
            .Include(x => x.Innings)
                .ThenInclude(x => x.Overs)
                    .ThenInclude(x => x.Deliveries)
            .Include(x => x.Innings)
                .ThenInclude(x => x.BattingScores)

            .Include(x => x.Innings)
                .ThenInclude(x => x.BowlingScores)
            .SingleOrDefaultAsync(
                x => x.Id == matchId,
                cancellationToken);
    }
}