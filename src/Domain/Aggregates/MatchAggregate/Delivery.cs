namespace Domain.Aggregates.MatchAggregate;

public sealed class Delivery
{
    // Internal Identity
    public Guid Id { get; private set; }

    // Ball Position
    public int SequenceNumber { get; private set; }
    public int BallNumberInOver { get; private set; }

    // Players
    public string StrikerId { get; private set; } = string.Empty;
    public string NonStrikerId { get; private set; } = string.Empty;
    public string BowlerId { get; private set; } = string.Empty;

    // Runs
    public int BatterRuns { get; private set; }
    public int TotalRuns { get; private set; }
    public ExtraType ExtraType { get; private set; }

    // Wicket
    public WicketType WicketType { get; private set; }
    public string? DismissedPlayerId { get; private set; }
    public string? FielderId { get; private set; }

    // Audit
    public DateTime Timestamp { get; private set; }

    private Delivery(){};

    private Delivery(
        int SequenceNumber,
        int BallNumberInOver,
        string StrikerId,
        string NonStrikerId,
        string BowlerId,
        int BatterRuns,
        int TotalRuns,
        ExtraType ExtraType,
        WicketType WicketType,
        string? DismissedPlayerId,
        string? FielderId
    ){
        if(TotalRuns<0)
            throw new ArgumentException("Runs cannot be negative.");

        // Derived Property
        public bool IsWicket =>
        WicketType != WicketType.None;
        
        public bool IsLegal =>
            ExtraType != ExtraType.Wide &&
            ExtraType != ExtraType.NoBall;
    };

}