namespace Domain.Aggregates.MatchAggregate;

public sealed record Delivery(
    Guid Id,
    Guid BatterId,
    Guid BowlerId,
    int TotalRuns,
    int BatterRuns,
    ExtraType Extra,
    WicketType WicketKind,
    Guid? DismissedPlayerId,
    Guid? FielderId,
    DateTimeOffset RecordedAt)
{
    public bool IsWicket =>
        WicketKind != WicketType.None;

    public bool IsLegal =>
        Extra is not ExtraType.Wide
        and not ExtraType.NoBall;
}