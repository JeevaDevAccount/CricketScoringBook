using Domain.Aggregates;

namespace Domain.Aggregates.MatchAggregate;

public sealed class MatchState
{
    public Guid MatchId { get; private set; }
    public string ActiveScorerId { get; private set; } = string.Empty;
    public bool IsMatchCompleted { get; private set; }

    // --- Innings & Targeting State ---
    public byte CurrentInnings { get; private set; } = 1; // 1 = First Innings, 2 = Second Innings
    public int FirstInningsScore { get; private set; } = 0;
    public int TargetToWin { get; private set; } = 0;
    public int MaxOvers { get; private set; }

    // --- Active Live Scoreboard (Resets per Innings) ---
    public int TotalRuns { get; private set; }
    public int Wickets { get; private set; }
    public int TotalBalls { get; private set; }

    // --- Live Tracking Identifiers ---
    public string ActiveStrikerId { get; private set; } = string.Empty;
    public string ActiveNonStrikerId { get; private set; } = string.Empty;
    public string ActiveBowlerId { get; private set; } = string.Empty;

    // --- Fixed 6-Ball Structural Buffer ---
    private readonly BallEvent[] _undoBuffer = new BallEvent[6];
    private int _bufferHead = -1;
    private int _historyCount = 0;

    public MatchState(Guid matchId, int maxOvers)
    {
        MatchId = matchId;
        MaxOvers = maxOvers;
    }

    public void InitializeFirstScorer(string scorerId)
    {
        if (IsMatchCompleted)
            throw new InvalidOperationException("Cannot assign a scorer to a finalized match.");
            
        if (!string.IsNullOrEmpty(ActiveScorerId))
            throw new InvalidOperationException("Match already has an active scorer.");

        ActiveScorerId = scorerId;
    }

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

    public bool IsScorerValid(string scorerId) 
        => !IsMatchCompleted && !string.IsNullOrEmpty(ActiveScorerId) && ActiveScorerId == scorerId;

    public void SetActivePlayers(string strikerId, string nonStrikerId, string bowlerId, string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Only the active scorer can set active players.");

        ActiveStrikerId = strikerId;
        ActiveNonStrikerId = nonStrikerId;
        ActiveBowlerId = bowlerId;
    }

    /// <summary>
    /// Swaps the match engine state over to the second innings.
    /// Clears active runs/wickets for the new team but safely stores the target in memory.
    /// </summary>
    public void TransitionToSecondInnings(string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Only the active scorer can change innings.");

        if (CurrentInnings != 1)
            throw new InvalidOperationException("Match is already in the second innings.");

        FirstInningsScore = TotalRuns;
        TargetToWin = TotalRuns + 1;
        CurrentInnings = 2;

        // Reset real-time scoreboard metrics for the chasing team
        TotalRuns = 0;
        Wickets = 0;
        TotalBalls = 0;

        // Clear the undo history array slots cleanly for the fresh innings
        Array.Clear(_undoBuffer, 0, _undoBuffer.Length);
        _bufferHead = -1;
        _historyCount = 0;
    }

    public void RecordBall(in BallEvent ball, string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException();

        // 1. Buffer tracking
        _bufferHead = (_bufferHead + 1) % 6;
        _undoBuffer[_bufferHead] = ball;
        if (_historyCount < 6) _historyCount++;

        if (ball.Extra == ExtraType.DeadBall) return;

        // 2. Process metrics
        TotalRuns += ball.TotalRunsAdded;
        if (ball.IsWicket) Wickets++;

        if (ball.Extra != ExtraType.Wide && 
            ball.Extra != ExtraType.NoBall && 
            ball.Extra != ExtraType.Penalty)
        {
            TotalBalls++;
        }

        if (ball.BatsmanRun % 2 != 0)
        {
            RotateStrike();
        }

        // --- Bulletproof Auto-Finalize Verification Conditions ---
        
        // Condition A: Chasing team passes target score in 2nd Innings
        if (CurrentInnings == 2 && TotalRuns >= TargetToWin)
        {
            IsMatchCompleted = true;
            ActiveScorerId = string.Empty;
            return;
        }

        // Condition B: Maximum overs reached for the innings
        int currentCompletedOvers = TotalBalls / 6;
        if (currentCompletedOvers >= MaxOvers)
        {
            if (CurrentInnings == 1)
            {
                // Auto-stop first innings so application knows it must transition to 2nd innings
                ActiveScorerId = string.Empty; 
            }
            else if (CurrentInnings == 2)
            {
                // Second innings completed without passing target -> Chasing team loses, game over!
                IsMatchCompleted = true;
                ActiveScorerId = string.Empty;
            }
        }
    }

    public void ExecuteUndo(string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Action rejected: You do not hold exclusive scoring rights.");

        if (_historyCount == 0 || _bufferHead == -1)
            throw new InvalidOperationException("Undo rejected: Buffer empty.");

        // If the match was auto-finalized by the previous ball, an undo re-opens it safely
        if (IsMatchCompleted)
        {
            IsMatchCompleted = false;
            ActiveScorerId = scorerId;
        }

        BallEvent lastBall = _undoBuffer[_bufferHead];

        _undoBuffer[_bufferHead] = default;
        _bufferHead = (_bufferHead - 1 + 6) % 6;
        _historyCount--;

        if (lastBall.Extra == ExtraType.DeadBall) return;

        TotalRuns -= lastBall.TotalRunsAdded;
        if (lastBall.IsWicket) Wickets--;

        if (lastBall.Extra != ExtraType.Wide && 
            lastBall.Extra != ExtraType.NoBall && 
            lastBall.Extra != ExtraType.Penalty)
        {
            TotalBalls--;
        }

        if (lastBall.BatsmanRun % 2 != 0)
        {
            RotateStrike();
        }
    }

    private void RotateStrike()
    {
        var temp = ActiveStrikerId;
        ActiveStrikerId = ActiveNonStrikerId;
        ActiveNonStrikerId = temp;
    }

    public void FinalizeMatch(string scorerId)
    {
        if (!IsScorerValid(scorerId))
            throw new UnauthorizedAccessException("Only the active scorer can manually finalize a match.");

        IsMatchCompleted = true;
        ActiveScorerId = string.Empty;
    }
}
