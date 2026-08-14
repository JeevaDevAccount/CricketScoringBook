namespace Domain.Aggregates.MatchAggregate;

public sealed class Over
{
    public Guid Id { get; private set; }

    public int OverNumber { get; private set; }

    public bool IsCompleted { get; private set; }
		
		public bool IsEditable { get; private set; } = true;
		
    private readonly List<Delivery> _deliveries = new();

    public IReadOnlyCollection<Delivery> Deliveries => _deliveries.AsReadOnly();
    
    private Over(){}
    
    private Over (int overNumber) { 
	    
	    if (overNumber <= 0)
	        throw new ArgumentException("Over Number must be greater than zero.");
	
	    Id = Guid.NewGuid();
	    OverNumber = overNumber;
    }
    
    public static Over Create(int overNumber){
	    return new Over(overNumber);
    }
    
    public void AddDelivery(Delivery delivery)
		{
	    if (delivery is null)
        throw new ArgumentNullException(nameof(delivery));
			if (!IsEditable)
		    throw new InvalidOperationException("This over is locked for editing.");
    
	    if (IsCompleted)
        throw new InvalidOperationException("Over is already completed.");
	
	    if (_deliveries.Any(d => d.Id == delivery.Id))
        throw new ArgumentException("Delivery has already been added.", nameof(delivery));
	
	    int legalDeliveryCount = GetLegalDeliveryCount();
	    int totalDeliveryCount = _deliveries.Count;
	
	    int expectedBallNumber = delivery.IsLegal
	        ? legalDeliveryCount + 1
	        : legalDeliveryCount;
	
	    if (delivery.BallNumberInOver != expectedBallNumber)
	        throw new ArgumentException(
	            $"Invalid BallNumberInOver. Expected {expectedBallNumber}.");
	
	    int expectedSequenceNumber = totalDeliveryCount + 1;
	
	    if (delivery.SequenceNumber != expectedSequenceNumber)
	        throw new ArgumentException(
	            $"Invalid SequenceNumber. Expected {expectedSequenceNumber}.");
	
	    _deliveries.Add(delivery);
	
	    if (GetLegalDeliveryCount() == 6)
	        MarkAsCompleted();
		}
    
    public Delivery UndoLastDelivery()
		{
		    if (_deliveries.Count == 0)
		        throw new InvalidOperationException("No delivery has been recorded to undo.");
				if (!IsEditable)
			    throw new InvalidOperationException("This over is locked for editing.");
    
		    Delivery delivery = _deliveries[^1];
		
		    _deliveries.RemoveAt(_deliveries.Count - 1);
		
		    if (IsCompleted && GetLegalDeliveryCount() < 6)
		        MarkAsInProgress();
		
		    return delivery;
		}
    
    private int GetLegalDeliveryCount()
    {
        return _deliveries.Count(d => d.IsLegal);
    }

    private void MarkAsCompleted()
		{
	    IsCompleted = true;
		}
		
		private void MarkAsInProgress()
		{
	    IsCompleted = false;
		}
		
		private void LockEditing()
		{
		    IsEditable = false;
		}
		
		private void UnlockEditing()
		{
		    IsEditable = true;
		}
		
		public bool IsMaiden => IsOverCompleted && Deliveries.Count == 6 && Deliveries.All(d => d.IsLegal) && Deliveries.All(d => d.BatterRuns == 0);
		
		public void RecordMaiden()
		{
		    Maidens++;
		}
		
		public void UndoMaiden()
		{
		    if (Maidens <= 0)
		        throw new InvalidOperationException(
		            "Invalid maiden count.");
		
		    Maidens--;
		}
}