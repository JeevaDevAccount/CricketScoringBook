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

    public async Task<Match?> GetByIdAsync(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        return await _context.Matches
            // Team 1 + Players
            .Include(x => x.Team1PlayingTeam)
                .ThenInclude(x => x.Players)

            // Team 2 + Players
            .Include(x => x.Team2PlayingTeam)
                .ThenInclude(x => x.Players)

            // Innings + Overs + Deliveries
            .Include(x => x.Innings)
                .ThenInclude(x => x.Overs)
                    .ThenInclude(x => x.Deliveries)

            .SingleOrDefaultAsync(
                x => x.Id == matchId,
                cancellationToken);
    }
}