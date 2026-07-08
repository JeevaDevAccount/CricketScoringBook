namespace Domain.Aggregates;

public enum WicketType
{
    None = 0,
    Bowled = 1,
    Caught = 2,
    LBW = 3,
    RunOut = 4,
    Stumped = 5,
    CaughtAndBowled = 6,
    HitWicket = 7
}

public enum ExtraType
{
    None = 0,       // Standard legal delivery (Dot ball, runs scored off bat)
    Wide = 1,       // Extra, does NOT count as a legal ball
    NoBall = 2,     // Extra, does NOT count as a legal ball
    Bye = 3,        // Extra, DOES count as a legal ball
    LegBye = 4,     // Extra, DOES count as a legal ball
    Penalty = 5,    // Strategic penalty runs, does NOT count as a legal ball
    DeadBall = 6    // Nullified delivery, does NOT count as a legal ball, zero stats impact
}

public enum MatchStatus
{
    Scheduled = 0,
    Live = 1,
    InningsBreak = 2,
    Completed = 3,
    Paused = 4
}