using Application.Abstractions.Interfaces;
using Application.Matches.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class PlayerQueries : IPlayerQueries
{
    private readonly AppDbContext _context;

    public PlayerQueries(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlayerLookupDto>> SearchPlayersAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var search = name.Trim();

        return await _context.Players
            .AsNoTracking()
            .Where(x => x.Name.Contains(search))
            .OrderBy(x => x.Name)
            .Select(x => new PlayerLookupDto(
                x.Id,
                x.Name))
            .Take(20)
            .ToListAsync(cancellationToken);
    }
}