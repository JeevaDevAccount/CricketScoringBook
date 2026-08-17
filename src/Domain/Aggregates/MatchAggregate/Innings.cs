namespace Domain.Aggregates.MatchAggregate;

public sealed class Innings
{
    public Guid Id { get; private set; }
    public int InningsNumber { get; private set; }
    public int BattingTeamId { get; private set; }
    public int BowlingTeamId { get; private set; }
	public InningsType Type { get; private set; }
    public int MaxOvers { get; private set; }
    public int? TargetRuns {get; private set;}
    public int TotalRuns { get; private set; }
    public int Wickets { get; private set; }
    public int TotalBalls { get; private set; }
    public int CurrentStrikerId { get; private set; }
    public int CurrentNonStrikerId { get; private set; }
    public int CurrentBowlerId { get; private set; }
    public bool IsCompleted { get; private set; }
    
    private readonly List<Over> _overs = new();
    public IReadOnlyCollection<Over> Overs => _overs.AsReadOnly();
    private Over CurrentOver => _overs[^1];
    
    private readonly Dictionary<int, BattingScore> _battingScores = [];
    private readonly Dictionary<int, BowlingScore> _bowlingScores = [];
    
    public IReadOnlyDictionary<int, BattingScore> BattingScores => _battingScores;
	public IReadOnlyDictionary<int, BowlingScore> BowlingScores => _bowlingScores;
    
    private Innings(){}
    
    private Innings(int inningsNumber,
		int battingTeamId,
		int bowlingTeamId,
		InningsType type,
		int maxOvers,
    	int? targetRuns,
		int currentStrikerId,
		int currentNonStrikerId,
		int currentBowlerId){
		
		if (inningsNumber <= 0)
	    	throw new ArgumentException("Innings number must be greater than zero.", nameof(inningsNumber));

		if (battingTeamId <= 0)
			throw new ArgumentException("Invalid batting team.", nameof(battingTeamId));

		if (bowlingTeamId <= 0)
			throw new ArgumentException("Invalid bowling team.", nameof(bowlingTeamId));

		if (battingTeamId == bowlingTeamId)
			throw new ArgumentException("Batting and bowling team cannot be the same.");

		if (currentStrikerId <= 0)
			throw new ArgumentException("Invalid striker.", nameof(currentStrikerId));

		if (currentNonStrikerId <= 0)
			throw new ArgumentException("Invalid non-striker.", nameof(currentNonStrikerId));

		if (currentBowlerId <= 0)
			throw new ArgumentException("Invalid bowler.", nameof(currentBowlerId));

		if (currentStrikerId == currentNonStrikerId)
			throw new ArgumentException("Striker and non-striker cannot be the same.");

		if (!Enum.IsDefined(type))
    		throw new ArgumentException("Invalid innings type.", nameof(type));
		if (maxOvers <= 0)
    		throw new ArgumentException("Maximum overs must be greater than zero.", nameof(maxOvers));
		if (targetRuns.HasValue && targetRuns.Value <= 0)
    		throw new ArgumentException( "Target runs must be greater than zero.", nameof(targetRuns));

		Id = Guid.NewGuid();
		InningsNumber = inningsNumber;
		BattingTeamId = battingTeamId;
		BowlingTeamId = bowlingTeamId;
		Type = type
		MaxOvers = maxOvers;
    	TargetRuns = targetRuns;
        
		CurrentStrikerId = currentStrikerId;
		CurrentNonStrikerId = currentNonStrikerId;
		CurrentBowlerId = currentBowlerId;
		
		_overs.Add(Over.Create(1));
	}
		
	public static Innings Create(
        int inningsNumber, int battingTeamId, int bowlingTeamId, InningsType type, int maxOvers, 
		int? targetRuns, int currentStrikerId, int currentNonStrikerId, int currentBowlerId)
    {
        return new Innings(
            inningsNumber,battingTeamId,bowlingTeamId,type,maxOvers,targetRuns,
            currentStrikerId,currentNonStrikerId,currentBowlerId
		);
    }
    
	public void RecordDelivery(Delivery delivery)
	{
		if (delivery is null) 
			throw new ArgumentNullException(nameof(delivery));
		
		if (IsCompleted)
			throw new InvalidOperationException("The innings has already been completed.");
			
		ValidateDelivery(delivery);
		CurrentOver.AddDelivery(delivery);
		
		BattingScore battingScore = GetOrCreateBattingScore(delivery.StrikerId); 
		BowlingScore bowlingScore = GetOrCreateBowlingScore(delivery.BowlerId);
				
		battingScore.RecordDelivery( delivery.BatterRuns, delivery.IsLegal, delivery.Dismissal); 
		
		bool creditedWithWicket = IsWicketCreditedToBowler(delivery);
		int bowlerRunsConceded = delivery.BatterRuns + BowlerExtraRuns(delivery);
		
		bowlingScore.RecordDelivery( bowlerRunsConceded, delivery.IsLegal, creditedWithWicket);
		
		UpdateScore(delivery);
		RotateStrikeForRuns(delivery);
	
		if (CurrentOver.IsOverCompleted)
		{
			RotateStrikeAtEndOfOver();
			if (CurrentOver.IsMaiden)
			{
				bowlingScore.RecordMaiden();
			}
			if (!IsCompleted)
				CreateNextOver();
		}
		CheckInningsCompletion();
	} 
		
	public void UndoLastDelivery(){
			
		if (IsCompleted)
		{
			IsCompleted = false;
		}
      
		RemoveEmptyCurrentOver();
		Over overToUndo = GetOverToUndo();

		if (overToUndo.IsMaiden)
	    {
	        bowlingScore.UndoMaiden();
	    }

		bool wasOverCompleted = OverToUndo.IsCompleted;
		Delivery removedDelivery = OverToUndo.UndoLastDelivery();
			
		if (wasOverCompleted)
			RotateStrikeAtEndOfOver();
			
		RotateStrikeForRuns(removedDelivery);
		UndoScore(removedDelivery);
			
		BattingScore battingScore = GetOrCreateBattingScore(removedDelivery.StrikerId); 
		BowlingScore bowlingScore = GetOrCreateBowlingScore(removedDelivery.BowlerId);
		
		int bowlerRunsConceded = CalculateBowlerRunsConceded(delivery);
		bool creditedWithWicket = IsWicketCreditedToBowler(delivery);
		
		battingScore.UndoDelivery( delivery.BatterRuns, delivery.IsLegal, delivery.Dismissal); 
		int bowlerRunsConceded = delivery.BatterRuns + BowlerExtraRuns;
		bowlingScore.UndoDelivery( bowlerRunsConceded, delivery.IsLegal, creditedWithWicket);
		UndoDismissedBatter(removedDelivery);
	} 
		
	private Over GetOverToUndo()
	{
		// Current over has deliveries
		if (CurrentOver.Deliveries.Any())
		{
			if (!CurrentOver.IsEditable)
				throw new InvalidOperationException("Current over is locked for editing.");
	
			return CurrentOver;
		}
	
		// No previous over exists
		if (_overs.Count < 2)
			throw new InvalidOperationException("No delivery available to undo.");
	
		Over previousOver = _overs[^2];
	
		if (!previousOver.IsEditable)
			throw new InvalidOperationException("Previous over is locked for editing.");
	
		return previousOver;
	}
		
	private void UpdateScore(Delivery delivery){
		if (delivery.TotalRuns > 0)
			TotalRuns += delivery.TotalRuns;
			
		if (delivery.IsWicket)
			Wickets += 1;
	
		if (delivery.IsLegal)
			TotalBalls += 1;
	}
		
	private void UndoScore(Delivery delivery){
		if (delivery.TotalRuns > 0)
			TotalRuns -= delivery.TotalRuns;
			
		if (delivery.IsWicket)
			Wickets -= 1;
	    
	    if (delivery.IsLegal)
		    TotalBalls -= 1;
	}
		
	private void RotateStrikeAtEndOfOver(){
		SwapStrike();
	}
		
	private void RotateStrikeForRuns(Delivery delivery){
		
		if (delivery.BatterRuns % 2 != 0 || (delivery.BatterRuns == 0 && delivery.TotalRuns % 2 != 0)){
			SwapStrike();
		}
	}
		
	private void SwapStrike()
    {
        int temp = CurrentStrikerId;
        CurrentStrikerId = CurrentNonStrikerId;
        CurrentNonStrikerId = temp;
    }
    
    public void ChangeBowler(int bowlerID){
	    if (bowlerID <= 0)
	      throw new ArgumentException("Invalid bowler.", nameof(bowlerID));
	    
		CurrentBowlerId = bowlerID;
    }
    
	private void CreateNextOver()
	{
		_overs.Add(Over.Create(_overs.Count + 1));
	}		
		
	private void RemoveEmptyCurrentOver()
	{
		// Never remove the very first over
		if (_overs.Count <= 1)
			return;
	
		// Remove only if the current over has no deliveries
		if (!CurrentOver.Deliveries.Any())
		{
			_overs.RemoveAt(_overs.Count - 1);
		}
	}
		
	private void ValidateDelivery(Delivery delivery) 
	{ 
		if (delivery.StrikerId != CurrentStrikerId) { 
			throw new InvalidOperationException( "Delivery striker does not match the current striker."); 
		} 
		
		if (delivery.NonStrikerId != CurrentNonStrikerId) { 
			throw new InvalidOperationException( "Delivery non-striker does not match the current non-striker."); 
		} 
		
		if (delivery.BowlerId != CurrentBowlerId) { 
			throw new InvalidOperationException( "Delivery bowler does not match the current bowler."); 
		}
	}
		
	private BattingScore GetOrCreateBattingScore(int playerId)
	{
		if (_battingScores.TryGetValue(playerId, out var score))
			return score;
		
		score = BattingScore.Create(playerId);
		_battingScores.Add(playerId, score);

		return score;
	}
		
	private BowlingScore GetOrCreateBowlingScore(int playerId)
	{
	    if (_bowlingScores.TryGetValue(playerId, out var score))
        	return score;

	    score = BowlingScore.Create(playerId);
	    _bowlingScores.Add(playerId, score);

	    return score;
	}
		
	private static int BowlerExtraRuns( Delivery delivery) 
	{
		switch (delivery.ExtraType)
		{
			case ExtraType.Wide:
				return 1;
			case ExtraType.Noball:
				return 1;
			default:
				return 0;
		}
	 }
		 
	private static bool IsWicketCreditedToBowler(Delivery delivery) 
	{ 
		if (delivery.Dismissal is null) return false; 
			return delivery.Dismissal.WicketType switch { WicketType.RunOut => false, _ => true }; 
	}
		 			
	public void ReplaceDismissedBatter(int dismissedPlayerId, int incomingBatterId)
	{
		if (dismissedPlayerId <= 0)
			throw new ArgumentException("Invalid dismissed player.",nameof(dismissedPlayerId));
	
		if (incomingBatterId <= 0)
			throw new ArgumentException("Invalid incoming batter.",nameof(incomingBatterId));
	
		if (dismissedPlayerId == incomingBatterId)
			throw new ArgumentException("Incoming batter cannot be the dismissed batter.",nameof(incomingBatterId));
	
		if (CurrentStrikerId == incomingBatterId || CurrentNonStrikerId == incomingBatterId)
		{
			throw new InvalidOperationException("Incoming batter is already on the field.");
		}
	
		if (CurrentStrikerId == dismissedPlayerId)
		{
			CurrentStrikerId = incomingBatterId;
			return;
		}
	
		if (CurrentNonStrikerId == dismissedPlayerId)
		{
			CurrentNonStrikerId = incomingBatterId;
			return;
		}
	
		throw new InvalidOperationException(
			"Dismissed player is not one of the current batters.");
	}
	
	private void UndoDismissedBatter(Delivery delivery)
	{
	    if (!delivery.IsWicket)
	        return;
	
	    int dismissedPlayerId = delivery.DismissedPlayerId;
	
	    if (delivery.StrikerId == dismissedPlayerId)
	    {
	        CurrentStrikerId = dismissedPlayerId;
	        return;
	    }
	
	    if (delivery.NonStrikerId == dismissedPlayerId)
	    {
	        CurrentNonStrikerId = dismissedPlayerId;
	        return;
	    }
	
	    throw new InvalidOperationException("Unable to restore dismissed batter.");
	}
	
	private void CheckInningsCompletion()
    {
        if (Wickets >= 10)
        {
            IsCompleted = true;
            return;
        }

        if (TargetRuns.HasValue && TotalRuns >= TargetRuns.Value)
        {
            IsCompleted = true;
            return;
        }

        if (TotalBalls >= MaxOvers * 6)
        {
            IsCompleted = true;
        }
    }
}