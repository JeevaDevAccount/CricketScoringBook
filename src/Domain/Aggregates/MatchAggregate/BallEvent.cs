namespace Domain.Aggregates.MatchAggregate;

/// <summary>
/// Represents an immutable transactional entry for a single ball delivered.
/// </summary>
public sealed record BallEvent(
    Guid Id, 
    int Runs, 
    bool IsWicket, 
    bool IsExtra, 
    string Description, 
    DateTime Timestamp);