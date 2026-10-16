using Application.Abstractions.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _context;

    public PlayerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Player?> GetByIdAsync(
        int playerId,
        CancellationToken cancellationToken)
    {
        return _context.Players
            .SingleOrDefaultAsync(
                x => x.Id == playerId,
                cancellationToken);
    }

    public Task<Player?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();

        return _context.Players
            .SingleOrDefaultAsync(
                x => x.Name == normalizedName,
                cancellationToken);
    }

    public void Add(Player player)
    {
        _context.Players.Add(player);
    }
}