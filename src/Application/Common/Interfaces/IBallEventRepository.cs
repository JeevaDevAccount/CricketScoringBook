using Domain.Aggregates.MatchAggregate;

namespace Application.Common.Interfaces;

public interface IBallEventRepository
{
    /// <summary>
    /// Asynchronously appends an immutable transactional ball event to the relational write path.
    /// Uses ValueTask to achieve zero-allocation overhead on high-frequency sequential writes.
    /// </summary>
    ValueTask AppendEventAsync(Guid matchId, BallEvent ballEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Flags a specific historical event as tombstoned/soft-deleted during an explicit Undo action.
    /// </summary>
    ValueTask TombstoneEventAsync(Guid eventId, CancellationToken cancellationToken);
}
