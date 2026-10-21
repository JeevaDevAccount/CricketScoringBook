using Application.Abstractions.Interfaces;

namespace Application.Matches.Queries.GetAllMatches;

public sealed class GetAllMatchesQueryHandler{
    private readonly IMatchQueries _matchQueries;

    public GetAllMatchesQueryHandler(IMatchQueries matchQueries){
        _matchQueries = matchQueries;
    }

    public async Task<IEnumerable<MatchDto?>> Handle(
        GetAllMatchesQuery query,
        CancellationToken cancellationToken
    ){
         return await _matchQueries.GetAllMatchesAsync(
            cancellationToken
         );
    }
}