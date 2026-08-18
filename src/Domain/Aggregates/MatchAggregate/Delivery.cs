using Domain.Enum;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Delivery
{
    public Guid Id { get; private set; }

    public int SequenceNumber { get; private set; }

    public int BallNumberInOver { get; private set; }

    public int StrikerId { get; private set; }

    public int NonStrikerId { get; private set; }

    public int BowlerId { get; private set; }

    public int BatterRuns { get; private set; }

    public int TotalRuns { get; private set; }

    public ExtraType ExtraType { get; private set; }

    public WicketType WicketType { get; private set; }

    public int? DismissedPlayerId { get; private set; }

    public int? FielderId { get; private set; }

    public DateTime Timestamp { get; private set; }

    public bool IsWicket =>
        WicketType != WicketType.None;

    public bool IsLegal =>
        ExtraType != ExtraType.Wide &&
        ExtraType != ExtraType.NoBall;

    private Delivery()
    {
    }

    private Delivery(
        int sequenceNumber,
        int ballNumberInOver,
        int strikerId,
        int nonStrikerId,
        int bowlerId,
        int batterRuns,
        int totalRuns,
        ExtraType extraType,
        WicketType wicketType,
        int? dismissedPlayerId,
        int? fielderId)
    {
        if (sequenceNumber <= 0)
            throw new ArgumentException(
                "Sequence number must be greater than zero.",
                nameof(sequenceNumber));

        if (ballNumberInOver <= 0)
            throw new ArgumentException(
                "Ball number must be greater than zero.",
                nameof(ballNumberInOver));

        if (strikerId <= 0)
            throw new ArgumentException(
                "Invalid striker.",
                nameof(strikerId));

        if (nonStrikerId <= 0)
            throw new ArgumentException(
                "Invalid non-striker.",
                nameof(nonStrikerId));

        if (bowlerId <= 0)
            throw new ArgumentException(
                "Invalid bowler.",
                nameof(bowlerId));

        if (strikerId == nonStrikerId)
            throw new ArgumentException(
                "Striker and non-striker cannot be the same.");

        if (batterRuns < 0)
            throw new ArgumentException(
                "Batter runs cannot be negative.",
                nameof(batterRuns));

        if (totalRuns < 0)
            throw new ArgumentException(
                "Total runs cannot be negative.",
                nameof(totalRuns));

        if (totalRuns < batterRuns)
            throw new ArgumentException(
                "Total runs cannot be less than batter runs.",
                nameof(totalRuns));

        if (wicketType == WicketType.None &&
            dismissedPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Dismissed player cannot be specified without a wicket.",
                nameof(dismissedPlayerId));
        }

        if (wicketType != WicketType.None &&
            !dismissedPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Dismissed player is required when a wicket is recorded.",
                nameof(dismissedPlayerId));
        }

        Id = Guid.NewGuid();

        SequenceNumber = sequenceNumber;
        BallNumberInOver = ballNumberInOver;

        StrikerId = strikerId;
        NonStrikerId = nonStrikerId;
        BowlerId = bowlerId;

        BatterRuns = batterRuns;
        TotalRuns = totalRuns;

        ExtraType = extraType;

        WicketType = wicketType;
        DismissedPlayerId = dismissedPlayerId;
        FielderId = fielderId;

        Timestamp = DateTime.UtcNow;
    }

    public static Delivery Create(
        int sequenceNumber,
        int ballNumberInOver,
        int strikerId,
        int nonStrikerId,
        int bowlerId,
        int batterRuns,
        int totalRuns,
        ExtraType extraType = ExtraType.None,
        WicketType wicketType = WicketType.None,
        int? dismissedPlayerId = null,
        int? fielderId = null)
    {
        return new Delivery(
            sequenceNumber,
            ballNumberInOver,
            strikerId,
            nonStrikerId,
            bowlerId,
            batterRuns,
            totalRuns,
            extraType,
            wicketType,
            dismissedPlayerId,
            fielderId);
    }
}