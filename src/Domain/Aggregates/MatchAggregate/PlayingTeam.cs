namespace Domain.Aggregates.MatchAggregate;

public sealed class PlayingTeam
{
    private const int MaximumPlayers = 11;

    private readonly List<int> _playerIds = [];

    public int TeamId { get; }

    public IReadOnlyList<int> Players => _playerIds.AsReadOnly();

    private PlayingTeam()
    {
    }

    private PlayingTeam(int teamId)
    {
        if (teamId <= 0)
            throw new ArgumentOutOfRangeException(nameof(teamId), "Invalid team id.");

        TeamId = teamId;
    }

    public static PlayingTeam Create(int teamId)
    {
        return new PlayingTeam(teamId);
    }

    public void AddPlayer(int playerId)
    {
        if (playerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(playerId), "Invalid player id.");

        if (ContainsPlayer(playerId))
            throw new InvalidOperationException("Player already exists in the playing team.");

        if (_playerIds.Count >= MaximumPlayers)
            throw new InvalidOperationException("A playing team cannot contain more than 11 players.");

        _playerIds.Add(playerId);
    }

    public bool ContainsPlayer(int playerId)
    {
        return _playerIds.Contains(playerId);
    }

    public int PlayerCount()
    {
        return _playerIds.Count;
    }
}