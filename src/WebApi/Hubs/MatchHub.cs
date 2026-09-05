using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public sealed class MatchHub : Hub
{
    public async Task JoinMatch(Guid matchId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(matchId));
    }

    public async Task LeaveMatch(Guid matchId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(matchId));
    }

    public static string GetGroupName(Guid matchId) => $"match-{matchId}";
}