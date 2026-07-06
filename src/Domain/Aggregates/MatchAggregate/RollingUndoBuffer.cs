namespace Domain.Aggregates.MatchAggregate;

/// <summary>
/// A clean, standard List-backed rolling buffer bounded strictly to 6 elements.
/// </summary>
public sealed class RollingUndoBuffer
{
    private readonly List<BallEvent> _history = new(MaxUndoCapacity);
    private const int MaxUndoCapacity = 6;

    public void Push(BallEvent ball)
    {
        if (_history.Count >= MaxUndoCapacity)
        {
            _history.RemoveAt(0); // Evict the oldest ball; shifts indexes 1-5 left by 1 position
        }
        _history.Add(ball); // Append the fresh ball entry to the end of the list
    }

    public BallEvent? Pop()
    {
        if (_history.Count == 0) return null;

        // In a List, the latest ball is natively at the very last index, which perfectly matches an Undo LIFO requirement!
        int lastIndex = _history.Count - 1;
        var lastBall = _history[lastIndex];
        
        _history.RemoveAt(lastIndex); // Extremely fast O(1) removal since no elements follow it
        return lastBall;
    }

    public int Count => _history.Count;
}