using Application.Matches.Queries;
using Domain.Entities;

namespace Application.Abstractions.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(int playerId,CancellationToken cancellationToken);

    Task<Player?> FindByNameAsync(string name,CancellationToken cancellationToken);

    void Add(Player player);
}

public interface IPlayerQueries
{
    Task<IReadOnlyList<PlayerLookupDto>> SearchPlayersAsync(string name,CancellationToken cancellationToken);
}