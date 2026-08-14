using Domain.Enum;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Match
{
    public Guid Id { get; private set; }

    public MatchStatus Status { get; private set; }

    public string ActiveScorerId { get; private set; } = string.Empty;

    public int MaxOvers { get; private set; }

    public int TargetToWin { get; private set; }

    private readonly List<Innings> _innings = new();

    public IReadOnlyCollection<Innings> Innings => _innings.AsReadOnly();
}