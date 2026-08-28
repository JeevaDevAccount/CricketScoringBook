namespace Domain.Aggregates.MatchAggregate;

public sealed class PlayingTeam
{
    private const int MaximumPlayers = 11;

    private readonly List<PlayingTeamPlayer> _players = [];

    public int TeamId { get; }

    public IReadOnlyCollection<PlayingTeamPlayer> Players =>
        _players.AsReadOnly();

    private PlayingTeam() { }

    private PlayingTeam(int teamId)
    {
        if (teamId <= 0)
            throw new ArgumentOutOfRangeException(nameof(teamId));

        TeamId = teamId;
    }

    public static PlayingTeam Create(int teamId)
        => new(teamId);

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

    public bool ContainsPlayer(int playerId)
        => _players.Any(x => x.PlayerId == playerId);

    public int PlayerCount()
        => _players.Count;
}