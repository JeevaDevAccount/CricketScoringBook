namespace Domain.Aggregates.MatchAggregate;

public sealed class PlayingTeamPlayer
{
    public int PlayerId { get; }

    private PlayingTeamPlayer()
    {
    }

    private PlayingTeamPlayer(int playerId)
    {
        if (playerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(playerId));

        PlayerId = playerId;
    }

    public static PlayingTeamPlayer Create(int playerId)
        => new(playerId);
}