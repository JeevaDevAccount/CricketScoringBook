using Domain.Enum;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Over
{
    public Guid Id { get; private set; }
    public int OverNumber { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsEditable { get; private set; } = true;

    private readonly List<Delivery> _deliveries = [];

    public IReadOnlyCollection<Delivery> Deliveries =>
        _deliveries.AsReadOnly();

    private Over()
    {
    }

    private Over(int overNumber)
    {
        if (overNumber <= 0)
            throw new ArgumentException(
                "Over number must be greater than zero.",
                nameof(overNumber));

        Id = Guid.NewGuid();
        OverNumber = overNumber;
    }

    public static Over Create(int overNumber)
        => new(overNumber);

    public Delivery AddDelivery(Match.DeliveryInput input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (!IsEditable)
            throw new InvalidOperationException(
                "This over is locked for editing.");

        if (IsCompleted)
            throw new InvalidOperationException(
                "Over is already completed.");

        int legalDeliveryCount = GetLegalDeliveryCount();
        int expectedSequenceNumber = _deliveries.Count + 1;

        bool isLegal =
            input.ExtraType != ExtraType.Wide &&
            input.ExtraType != ExtraType.NoBall;

        int expectedBallNumber = isLegal
            ? legalDeliveryCount + 1
            : legalDeliveryCount;

        Delivery delivery = Delivery.Create(
            expectedSequenceNumber,
            expectedBallNumber,
            input.StrikerId,
            input.NonStrikerId,
            input.BowlerId,
            input.BatterRuns,
            input.TotalRuns,
            input.ExtraType,
            input.Dismissal,
            input.DismissedPlayerId);

        _deliveries.Add(delivery);

        if (GetLegalDeliveryCount() == 6)
            MarkAsCompleted();

        return delivery;
    }

    public Delivery UndoLastDelivery()
    {
        if (_deliveries.Count == 0)
            throw new InvalidOperationException(
                "No delivery has been recorded to undo.");

        if (!IsEditable)
            throw new InvalidOperationException(
                "This over is locked for editing.");

        Delivery delivery = _deliveries[^1];

        _deliveries.RemoveAt(_deliveries.Count - 1);

        if (IsCompleted && GetLegalDeliveryCount() < 6)
            MarkAsInProgress();

        return delivery;
    }

    public bool IsMaiden =>
        IsCompleted &&
        _deliveries.Count == 6 &&
        _deliveries.All(d => d.IsLegal) &&
        _deliveries.All(d => d.BatterRuns == 0);

    private int GetLegalDeliveryCount()
        => _deliveries.Count(d => d.IsLegal);

    private void MarkAsCompleted()
        => IsCompleted = true;

    private void MarkAsInProgress()
        => IsCompleted = false;

    public void LockEditing()
        => IsEditable = false;

    public void UnlockEditing()
        => IsEditable = true;
}