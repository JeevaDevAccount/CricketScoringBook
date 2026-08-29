using Application.Abstractions.Interfaces;

namespace Application.Matches.Queries.GetMatch;

public sealed class GetMatchQueryHandler{
    private readonly IMatchQueries _matchQueries;

    public GetMatchQueryHandler(IMatchQueries matchQueries){
        _matchQueries = matchQueries;
    }

    public async Task<MatchDto?> Handle(
       GetMatchQuery query,
        CancellationToken cancellationToken
    ){
         return await _matchQueries.GetMatchAsync(
            query.MatchId,
            cancellationToken
         );
    }
}