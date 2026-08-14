using Domain.Enum;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Delivery
{
	// Internal Identity
	public Guid Id { get; private set; }
	
	// Ball Position
	public int SequenceNumber { get; private set; }
	public int BallNumberInOver { get; private set; }
	
	// Players
	public string StrikerId { get; private set; } = string.Empty;
	public string NonStrikerId { get; private set; } = string.Empty;
	public string BowlerId { get; private set; } = string.Empty;
	
	// Runs
	public int BatterRuns { get; private set; }
	public int TotalRuns { get; private set; }
	public ExtraType ExtraType { get; private set; }
	
	// Wicket
	public int? DismissedPlayerId { get; private set; }
	public Dismissal Dismissal {get; private set;}	
	
	// Audit
	public DateTime Timestamp { get; private set; }
	
	// Derived Properties
	public bool IsWicket => WicketType != WicketType.None;
	
	public bool IsLegal =>
	    ExtraType != ExtraType.Wide &&
	    ExtraType != ExtraType.NoBall;
	
	// Required by EF Core
	private Delivery()
	{
	}

	private Delivery(
	    int sequenceNumber,
	    int ballNumberInOver,
	    string strikerId,
	    string nonStrikerId,
	    string bowlerId,
	    int batterRuns,
	    int totalRuns,
	    ExtraType extraType,
	    string? dismissedPlayerId,
	    Dismissal? dismissal)
	{
	    Validate(
	        sequenceNumber,
	        ballNumberInOver,
	        strikerId,
	        nonStrikerId,
	        bowlerId,
	        batterRuns,
	        totalRuns,
	        dismissedPlayerId,
	        dismissal);
	
	    Id = Guid.NewGuid();
	    Timestamp = DateTime.UtcNow;
	
	    SequenceNumber = sequenceNumber;
	    BallNumberInOver = ballNumberInOver;
	
	    StrikerId = strikerId;
	    NonStrikerId = nonStrikerId;
	    BowlerId = bowlerId;
	
	    BatterRuns = batterRuns;
	    TotalRuns = totalRuns;
	    ExtraType = extraType;
	
	    DismissedPlayerId = dismissedPlayerId;
	    Dismissal = dismissal;
	}

	public static Delivery Create(
	    int sequenceNumber,
	    int ballNumberInOver,
	    string strikerId,
	    string nonStrikerId,
	    string bowlerId,
	    int batterRuns,
	    int totalRuns,
	    ExtraType extraType,
	    int? dismissedPlayerId,
	    Dismissal? dismissal)
	{
	    return new Delivery(
	        sequenceNumber,
	        ballNumberInOver,
	        strikerId,
	        nonStrikerId,
	        bowlerId,
	        batterRuns,
	        totalRuns,
	        extraType,
	        dismissedPlayerId,
	        dismissal);
	}

	private static void Validate(
	    int sequenceNumber,
	    int ballNumberInOver,
	    string strikerId,
	    string nonStrikerId,
	    string bowlerId,
	    int batterRuns,
	    int totalRuns,
	    int? dismissedPlayerId,
	    Dismissal? dismissal)
	{
	    ValidateBall(sequenceNumber, ballNumberInOver);
	    ValidatePlayers(strikerId, nonStrikerId, bowlerId);
	    ValidateRuns(batterRuns, totalRuns);
	    ValidateDismissal( dismissedPlayerId, dismissal);
	}

	private static void ValidateBall(
	    int sequenceNumber,
	    int ballNumberInOver)
	{
	    if (sequenceNumber <= 0)
	        throw new ArgumentException("Sequence number must be greater than zero.");
	
	    if (ballNumberInOver <= 0)
	        throw new ArgumentException("Ball number in the over must be greater than zero.");
	}

	private static void ValidatePlayers(
	    string strikerId,
	    string nonStrikerId,
	    string bowlerId)
	{
	    if (string.IsNullOrWhiteSpace(strikerId))
	        throw new ArgumentException("Striker is required.");
	
	    if (string.IsNullOrWhiteSpace(nonStrikerId))
	        throw new ArgumentException("Non-striker is required.");
	
	    if (string.IsNullOrWhiteSpace(bowlerId))
	        throw new ArgumentException("Bowler is required.");
	}

	private static void ValidateRuns(
	    int batterRuns,
	    int totalRuns)
	{
	    if (batterRuns < 0)
	        throw new ArgumentException("Batter runs cannot be negative.");
	
	    if (totalRuns < 0)
	        throw new ArgumentException("Total runs cannot be negative.");
	
	    if (totalRuns < batterRuns)
	        throw new ArgumentException("Total runs cannot be less than batter runs.");
	}
	
	private static void ValidateDismissal( int? dismissedPlayerId, Dismissal? dismissal) 
	{ 
		if (dismissal is null) 
		{ 
			if (dismissedPlayerId.HasValue) 
			{ 
				throw new ArgumentException( "Dismissed player cannot be specified when there is no dismissal.", nameof(dismissedPlayerId)); 
			} 
			return; 
		} 
		
		if (!dismissedPlayerId.HasValue || dismissedPlayerId <= 0) 
		{ 
			throw new ArgumentException( "Dismissed player is required when a dismissal occurs.", nameof(dismissedPlayerId)); 
		} 
	}
	
	private static void ValidateWicket(
	    WicketType wicketType,
	    string? dismissedPlayerId,
	    string? fielderId)
	{
	    bool isWicket = wicketType != WicketType.None;
	
	    if (!isWicket)
	    {
	        if (!string.IsNullOrWhiteSpace(dismissedPlayerId))
	            throw new ArgumentException("Dismissed player is not allowed when no wicket has fallen.");
	
	        if (!string.IsNullOrWhiteSpace(fielderId))
	            throw new ArgumentException("Fielder is not allowed when no wicket has fallen.");
	
	        return;
	    }
	
	    if (string.IsNullOrWhiteSpace(dismissedPlayerId))
	        throw new ArgumentException("Dismissed player is required for a wicket.");
	
	    bool requiresFielder =
	        wicketType == WicketType.Caught ||
	        wicketType == WicketType.RunOut ||
	        wicketType == WicketType.Stumped;
	
	    if (requiresFielder && string.IsNullOrWhiteSpace(fielderId))
	        throw new ArgumentException("Fielder is required for this wicket type.");
	}
}