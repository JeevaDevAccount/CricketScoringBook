namespace Domain.Enums;

public enum ExtraType
{
    None = 0,       // Standard legal delivery (Dot ball, runs scored off bat)
    Wide = 1,       // Extra, does NOT count as a legal ball
    NoBall = 2,     // Extra, does NOT count as a legal ball
    Bye = 3,        // Extra, DOES count as a legal ball
    LegBye = 4,     // Extra, DOES count as a legal ball
    Penalty = 5,    // Strategic penalty runs, does NOT count as a legal ball
}