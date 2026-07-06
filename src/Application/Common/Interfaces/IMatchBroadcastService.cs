using Domain.Aggregates.MatchAggregate;

namespace Application.Common.Interfaces;

public interface IMatchBroadcastService
{
    /// <summary>
    /// Blasts a lightweight compressed update payload directly to the isolated real-time streaming group.
    /// </summary>
    ValueTask BroadcastBallUpdateAsync(Guid matchId, BallEvent ballDelta, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies all real-time viewers that an Undo action was executed, forcing client-side score resyncs.
    /// </summary>
    ValueTask BroadcastUndoNotificationAsync(Guid matchId, CancellationToken cancellationToken);
}
