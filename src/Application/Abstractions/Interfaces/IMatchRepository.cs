using Domain.Aggregates.MatchAggregate;
using Application.Matches.Queries;

namespace Application.Abstractions.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync (Guid matchId, CancellationToken cancellationToken);
}

public interface IMatchQueries
{
    Task<MatchDto?> GetMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken);

    Task<LiveMatchDto?> GetLiveMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken);

    Task<ScorecardDto?> GetScorecardAsync(
        Guid matchId,
        CancellationToken cancellationToken);
}