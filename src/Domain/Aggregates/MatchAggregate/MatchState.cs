namespace Domain.Aggregates.MatchAggregate;

/// <summary>
/// The aggregate root tracking live match state. Completely independent of databases or APIs.
/// </summary>
public sealed class MatchState
{
    public Guid MatchId { get; private set; }
    public int TotalRuns { get; private set; }
    public int Wickets { get; private set; }
    public int TotalBalls { get; private set; }
    public string ActiveScorerId { get; private set; } = string.Empty;
    public DateTime LeaseExpiry { get; private set; }
    public bool IsMatchCompleted { get; private set; }

    private readonly RollingUndoBuffer _undoBuffer = new();

    public MatchState(Guid matchId)
    {
        MatchId = matchId;
    }

    public void AssignLease(string scorerId, TimeSpan duration)
    {
        if (IsMatchCompleted) 
            throw new InvalidOperationException("Cannot assign scorer leases to a finalized match.");
            
        ActiveScorerId = scorerId;
        LeaseExpiry = DateTime.UtcNow.Add(duration);
    }

    public bool IsLeaseValid(string scorerId) 
        => !IsMatchCompleted && ActiveScorerId == scorerId && DateTime.UtcNow < LeaseExpiry;

    public void RecordBall(BallEvent ball, string scorerId)
    {
        if (!IsLeaseValid(scorerId))
            throw new UnauthorizedAccessException("Action rejected: Scorer lease is invalid or expired.");

        TotalRuns += ball.Runs;
        if (ball.IsWicket) Wickets++;
        if (!ball.IsExtra) TotalBalls++;

        _undoBuffer.Push(ball);
    }

    public void ExecuteUndo(string scorerId)
    {
        if (!IsLeaseValid(scorerId))
            throw new UnauthorizedAccessException("Action rejected: Scorer lease is invalid or expired.");

        var lastBall = _undoBuffer.Pop();
        if (lastBall == null)
            throw new InvalidOperationException("Undo rejected: Explicit 6-ball rolling buffer limit is empty.");

        TotalRuns -= lastBall.Runs;
        if (lastBall.IsWicket) Wickets--;
        if (!lastBall.IsExtra) TotalBalls--;
    }

    public void FinalizeMatch(string scorerId)
    {
        if (!IsLeaseValid(scorerId))
            throw new UnauthorizedAccessException("Only the active session leasing scorer can finalize a match.");

        IsMatchCompleted = true;
        ActiveScorerId = string.Empty;
    }
}