namespace Application.Matches.Queries.GetMatch;

public sealed class GetScorecardQueryHandler(){
    private readonly IMatchQueries _matchQueries;

    public sealed GetScorecardQueryHandler(IMatchQueries matchQueries){
        _matchQueries = matchQueries;
    }

    public async Task<ScorecardDto?> Handle(
        GetScorecardQuery query,
        CancellationToken cancellationToken
    ){
        return await _matchQueries.GetScorecardAsync(
            query.MatchId,
            cancellationToken
        );
    }
}