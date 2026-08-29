namespace Application.Abstractions.Interfaces;
using Domain.Aggregates.MatchAggregate;
using Application.Matches.Queries;

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