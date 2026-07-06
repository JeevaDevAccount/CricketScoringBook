namespace Domain.Aggregates.MatchAggregate;

/// <summary>
/// A memory-bounded ring buffer ensuring lookbacks never exceed the 6-ball undo limit.
/// </summary>
public sealed class RollingUndoBuffer
{
    private readonly LinkedList<BallEvent> _history = new();
    private const int MaxUndoCapacity = 6;

    public void Push(BallEvent ball)
    {
        if (_history.Count >= MaxUndoCapacity)
        {
            _history.RemoveFirst(); // Evict oldest ball from heap to protect the 512MB RAM ceiling
        }
        _history.AddLast(ball);
    }

    public BallEvent? Pop()
    {
        if (_history.Count == 0) return null;
        var lastNode = _history.Last!;
        _history.RemoveLast();
        return lastNode.Value;
    }

    public int Count => _history.Count;
}