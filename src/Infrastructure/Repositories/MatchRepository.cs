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
        var match = await _context.Matches
            .Include(x => x.Innings)
                .ThenInclude(x => x.Overs)
                    .ThenInclude(x => x.Deliveries)
            .SingleOrDefaultAsync(
                x => x.Id == matchId,
                cancellationToken);

        if (match is null)
            return null;

        var playerRows = await _context.PlayingTeamPlayers
            .Where(x => x.MatchId == matchId)
            .ToListAsync(cancellationToken);

        // Reconstruct Team 1 players
        foreach (var player in playerRows
            .Where(x => x.TeamId == match.Team1PlayingTeam.TeamId))
        {
            if (!match.Team1PlayingTeam.ContainsPlayer(player.PlayerId))
                match.Team1PlayingTeam.AddPlayer(player.PlayerId);
        }

        // Reconstruct Team 2 players
        foreach (var player in playerRows
            .Where(x => x.TeamId == match.Team2PlayingTeam.TeamId))
        {
            if (!match.Team2PlayingTeam.ContainsPlayer(player.PlayerId))
                match.Team2PlayingTeam.AddPlayer(player.PlayerId);
        }

        return match;
    }
}