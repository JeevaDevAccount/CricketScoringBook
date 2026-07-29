namespace Domain.Aggregates.MatchAggregate;

public sealed class Innings
{
    public Guid Id { get; private set; }

    public int InningsNumber { get; private set; }

    public string BattingTeamId { get; private set; } = string.Empty;

    public string BowlingTeamId { get; private set; } = string.Empty;

    public int TotalRuns { get; private set; }

    public int Wickets { get; private set; }

    public int TotalBalls { get; private set; }

    public string CurrentStrikerId { get; private set; } = string.Empty;

    public string CurrentNonStrikerId { get; private set; } = string.Empty;

    public string CurrentBowlerId { get; private set; } = string.Empty;

    private readonly List<Over> _overs = new();

    public IReadOnlyCollection<Over> Overs => _overs.AsReadOnly();
}