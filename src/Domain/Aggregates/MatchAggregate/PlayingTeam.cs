namespace Domain.Aggregates.MatchAggregate;

public sealed class PlayingTeam
{
    private const int MaximumPlayers = 11;
    private readonly List<PlayingTeamPlayer> _players = [];

    // 🌟 THE PERMANENT DOMAIN FIX: Composite identity properties
    public Guid MatchId { get; private set; }
    public int TeamId { get; }

    public IReadOnlyCollection<PlayingTeamPlayer> Players =>
        _players.AsReadOnly();

    private PlayingTeam() { }

    private PlayingTeam(Guid matchId, int teamId)
    {
        if (teamId <= 0)
            throw new ArgumentOutOfRangeException(nameof(teamId));

        MatchId = matchId;
        TeamId = teamId;
    }

    public static PlayingTeam Create(Guid matchId, int teamId)
        => new(matchId, teamId);

    public void AddPlayer(int playerId)
    {
        if (playerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(playerId));

        if (_players.Any(x => x.PlayerId == playerId))
            throw new InvalidOperationException("Player already exists.");

        if (_players.Count >= MaximumPlayers)
            throw new InvalidOperationException(
                "A playing team cannot contain more than 11 players.");

        _players.Add(PlayingTeamPlayer.Create(playerId));
    }

    public bool ContainsPlayer(int playerId) => _players.Any(x => x.PlayerId == playerId);
    public int PlayerCount() => _players.Count;
}
