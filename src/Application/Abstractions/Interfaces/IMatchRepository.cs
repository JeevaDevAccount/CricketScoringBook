namespace Application.Abstractions.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync (Guid matchId, CancellationToken cancellationToken);
}
