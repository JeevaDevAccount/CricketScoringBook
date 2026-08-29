using Application.Abstractions.Interfaces;

namespace Application.Matches.Queries.GetLiveMatch;

public sealed class GetLiveMatchQueryHandler
{
    private readonly IMatchQueries _matchQueries;

    public GetLiveMatchQueryHandler(IMatchQueries matchQueries)
    {
        _matchQueries = matchQueries;
    }

    public async Task<LiveMatchDto?> Handle(
        GetLiveMatchQuery query,
        CancellationToken cancellationToken)
    {
        return await _matchQueries.GetLiveMatchAsync(
            query.MatchId,
            cancellationToken);
    }
}