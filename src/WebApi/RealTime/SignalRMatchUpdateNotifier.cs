using Application.Abstractions.Interfaces;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.RealTime;

public sealed class SignalRMatchUpdateNotifier : IMatchUpdateNotifier
{
    private readonly IHubContext<MatchHub> _hubContext;
    private readonly IMatchQueries _matchQueries;

    public SignalRMatchUpdateNotifier(
        IHubContext<MatchHub> hubContext,
        IMatchQueries matchQueries)
    {
        _hubContext = hubContext;
        _matchQueries = matchQueries;
    }

    public async Task NotifyMatchUpdatedAsync(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        var liveMatch = await _matchQueries.GetLiveMatchAsync(
            matchId,
            cancellationToken);

        if (liveMatch is null)
            return;

        await _hubContext.Clients
            .Group(MatchHub.GetGroupName(matchId))
            .SendAsync(
                "MatchUpdated",
                liveMatch,
                cancellationToken);
    }
}