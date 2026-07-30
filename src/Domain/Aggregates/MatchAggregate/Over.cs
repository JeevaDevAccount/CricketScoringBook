namespace Domain.Aggregates.MatchAggregate;

public sealed class Over
{
    public Guid Id { get; private set; }

    public int OverNumber { get; private set; }

    public bool IsLocked { get; private set; }

    private readonly List<Delivery> _deliveries = new();

    public IReadOnlyCollection<Delivery> Deliveries => _deliveries.AsReadOnly();
}