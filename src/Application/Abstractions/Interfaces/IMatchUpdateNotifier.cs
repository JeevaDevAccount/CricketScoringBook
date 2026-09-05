namespace Application.Abstractions.Interfaces;

public interface IMatchUpdateNotifier
{
    Task NotifyMatchUpdatedAsync(
        Guid matchId,
        CancellationToken cancellationToken);
}