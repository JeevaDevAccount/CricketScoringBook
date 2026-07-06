namespace Domain.Aggregates.MatchAggregate;

/// <summary>
/// The aggregate root tracking live match state. 
/// Encapsulates all cricketing laws and fluid scorer session ownership under strict memory constraints.
/// </summary>
public sealed class MatchState
{
    public Guid MatchId { get; private set; }
    public int TotalRuns { get; private set; }
    public int Wickets { get; private set; }
    public int TotalBalls { get; private set; }
    public string ActiveScorerId { get; private set; } = string.Empty;
    public bool IsMatchCompleted { get; private set; }

    private readonly RollingUndoBuffer _undoBuffer = new();

    public MatchState(Guid matchId)
    {
        MatchId = matchId;
    }

    /// <summary>
    /// Initializes the very first scorer for the match. Only allowed if no scorer is currently assigned.
    /// </summary>
    public void InitializeFirstScorer(string scorerId)
    {
        if (IsMatchCompleted)
            throw new InvalidOperationException("Cannot assign a scorer to a finalized match.");
            
        if (!string.IsNullOrEmpty(ActiveScorerId))
            throw new InvalidOperationException("Match already has an active scorer. Use the handover process instead.");

        ActiveScorerId = scorerId;
    }

    /// <summary>
    /// Smoothly handovers exclusive scoring rights to another user.
    /// Crucially, ONLY the currently active scorer can initiate this handover.
    /// </summary>
    public void HandoverOwnership(string currentScorerId, string newScorerId)
    {
        if (IsMatchCompleted) 
            throw new InvalidOperationException("Cannot transfer scoring rights for a finalized match.");

        if (!IsScorerValid(currentScorerId))
            throw new UnauthorizedAccessException("Action rejected: Only the current active scorer can handover ownership.");

        if (string.IsNullOrWhiteSpace(newScorerId))
            throw new ArgumentException("New scorer identity cannot be empty.", nameof(newScorerId));

        ActiveScorerId = newScorerId;
    }

    /// <summary>
    /// Evaluates if the requesting user is the current exclusive owner of this match session.
    /// </summary>
    public bool IsScorerValid(string scorerId) 
        => !IsMatchCompleted && !string.IsNullOrEmpty(ActiveScorerId) && ActiveScorerId == scorerId;

    /// <summary>
    /// Main transaction lane entry point. Evaluates and transforms match metrics based on cricketing laws.
    /// </summary>
    public void RecordBall(BallEvent ball, string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Action rejected: You do not currently hold exclusive scoring rights for this match.");

        if (ball.Extra == ExtraType.DeadBall)
        {
            _undoBuffer.Push(ball);
            return;
        }

        TotalRuns += ball.Runs;
        if (ball.IsWicket) Wickets++;

        if (ball.Extra != ExtraType.Wide && 
            ball.Extra != ExtraType.NoBall && 
            ball.Extra != ExtraType.Penalty)
        {
            TotalBalls++;
        }

        _undoBuffer.Push(ball);
    }

    /// <summary>
    /// Reverses the last scoring transaction backward in state memory up to a hard ceiling of 6 balls.
    /// </summary>
    public void ExecuteUndo(string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Action rejected: You do not currently hold exclusive scoring rights for this match.");

        var lastBall = _undoBuffer.Pop();
        if (lastBall == null)
            throw new InvalidOperationException("Undo rejected: Explicit 6-ball rolling buffer limit is empty.");

        if (lastBall.Extra == ExtraType.DeadBall)
        {
            return;
        }

        TotalRuns -= lastBall.Runs;
        if (lastBall.IsWicket) Wickets--;

        if (lastBall.Extra != ExtraType.Wide && 
            lastBall.Extra != ExtraType.NoBall && 
            lastBall.Extra != ExtraType.Penalty)
        {
            TotalBalls--;
        }
    }

    /// <summary>
    /// Finalizes the active match, completely locking out further scoring modifications.
    /// </summary>
    public void FinalizeMatch(string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Action rejected: Only the active scorer can finalize a match.");

        IsMatchCompleted = true;
        ActiveScorerId = string.Empty;
    }
}