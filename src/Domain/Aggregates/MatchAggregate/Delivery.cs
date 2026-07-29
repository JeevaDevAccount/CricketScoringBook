namespace Domain.Aggregates.MatchAggregate;

public sealed class Delivery
{
    public Guid Id { get; private set; }

    public string StrikerId { get; private set; } = string.Empty;

    public string BowlerId { get; private set; } = string.Empty;

    public int BatterRuns { get; private set; }

    public int TotalRuns { get; private set; }

    public ExtraType ExtraType { get; private set; }

    public bool IsWicket { get; private set; }

    public WicketType WicketType { get; private set; }

    public string? DismissedPlayerId { get; private set; }

    public string? FielderId { get; private set; }

    public DateTime Timestamp { get; private set; }

    public bool IsLegal =>
        ExtraType != ExtraType.Wide &&
        ExtraType != ExtraType.NoBall;
}