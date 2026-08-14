namespace Domain.Aggregates.MatchAggregate;

public sealed class BattingScore
{
	public int PlayerId { get; }
	public int Runs { get; private set; }
	public int Balls { get; private set; }
	public int Fours { get; private set; }
	public int Sixes { get; private set; }
	public Dismissal? Dismissal { get; private set; }
	public bool IsOut => Dismissal != null;

	public decimal StrikeRate => Balls == 0 ? 0 : Math.Round((decimal)Runs * 100 / Balls, 2);
	
  private BattingScore(){}
  
  private BattingScore(int playerId)
  {
	  if (playerId <= 0)
	    throw new ArgumentException("Invalid player Id.", nameof(playerId));
	  PlayerId = playerId;
  }
  
  public static BattingScore Create(int playerId){
	  return new BattingScore(playerId);
	}
	
	public void RecordDelivery(int batterRuns, bool countAsBall, Dismissal dismissal){
		if (batterRuns < 0)
      throw new ArgumentOutOfRangeException(nameof(batterRuns));
    
    Runs += batterRuns;
    
    if (countAsBall)
	    Balls++;
    
    if(batterRuns == 4)
			Fours++;
		
		if(batterRuns == 6)
			Sixes++;
			
		if (dismissal is not null)
    {
      if (IsOut)
        throw new InvalidOperationException("Batter is already dismissed.");
        Dismissal = dismissal;
    }
}
	
	public void UndoDelivery(int batterRuns, bool countedAsBall, Dismissal? dismissal){
		if (batterRuns < 0)
      throw new ArgumentOutOfRangeException(nameof(batterRuns));

    if (Runs < batterRuns)
      throw new InvalidOperationException("Invalid batting state.");

    Runs -= batterRuns;

    if (countedAsBall)
    {
      if (Balls == 0)
          throw new InvalidOperationException("Invalid batting state.");
      Balls--;
    }

    if (batterRuns == 4)
    {
      if (Fours == 0)
        throw new InvalidOperationException("Invalid batting state.");
      Fours--;
    }

    if (batterRuns == 6)
    {
      if (Sixes == 0)
        throw new InvalidOperationException("Invalid batting state.");
      Sixes--;
    }

    if (dismissal is not null)
    {
      if (!IsOut)
        throw new InvalidOperationException("Batter is not dismissed.");
      Dismissal = null;
    }
	}
}