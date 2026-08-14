namespace Domain.Aggregates.MatchAggregate;

public sealed class BowlingScore
{
	public int PlayerId { get; }
	public int RunsConceded { get; private set; }
	public int Balls { get; private set; }
	public int Maidens { get; private set; }
	public int TotalWickets { get; private set; }
	
	public decimal Economy => Balls == 0 ? 0 : Math.Round((decimal)RunsConceded * 6 / Balls, 2);
	
  private BowlingScore(){}
  
  private BowlingScore(int playerId)
  {
	  if (playerId <= 0)
	    throw new ArgumentException("Invalid player Id.", nameof(playerId));
	  PlayerId = playerId;
  }
  
  public static BowlingScore Create(int playerId){
	  return new BowlingScore(playerId);
	}
	
	public void RecordDelivery(int runsConceded, bool countAsBall, bool creditedWithWicket){
		if (runsConceded < 0)
      throw new ArgumentOutOfRangeException(nameof(runsConceded), "Runs conceded cannot be negative.");
    
    RunsConceded += runsConceded;
    
    if (countAsBall)
	    Balls++;

    if (creditedWithWicket)
      TotalWickets++;
    
}
	
	public void UndoDelivery(int runsConceded, bool countAsBall, bool creditedWithWicket){
		if (runsConceded < 0)
      throw new ArgumentOutOfRangeException(nameof(runsConceded), "Runs conceded cannot be negative.");

    if (RunsConceded < runsConceded)
      throw new InvalidOperationException("Invalid bowling state.");

    RunsConceded -= runsConceded;

    if (countAsBall)
    {
      if (Balls == 0)
          throw new InvalidOperationException("Invalid batting state.");
      Balls--;
    }

    if (creditedWithWicket)
      TotalWickets--;
	}
	
	public void RecordMaiden(){
		Maidens++;
	}
	
	public void UndoMaiden(){
		if (Maidens <= 0)
      throw new InvalidOperationException("Invalid bowling state.");
		Maidens--;
	}
}