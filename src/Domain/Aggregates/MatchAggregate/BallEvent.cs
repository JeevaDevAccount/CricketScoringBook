namespace Domain.Aggregates.MatchAggregate;

/// <summary>
/// Represents an immutable transactional entry for a single ball delivered.
/// Readonly struct guarantees stack allocation and zero GC heap pressure.
/// </summary>
public readonly record struct BallEvent(
    Guid Id, 
    int TotalRunsAdded,     // The absolute total runs added to the team score (e.g., 4 for a wide + 3 runs)
    int BatsmanRun,         // The exact number of times the batters physically crossed ends
    ExtraType Extra,
    bool IsWicket,
    WicketType WicketKind,
    string DismissedPlayerId,
    string FielderId,
    DateTime Timestamp
);
