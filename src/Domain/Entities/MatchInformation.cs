namespace Domain.Entities;

public sealed class MatchInformation
{
    public Guid MatchId { get; set; }
    public Guid TeamAId { get; set; }
    public Guid TeamBId { get; set; }
    
    // Venue details
    public string Venue { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Toss Information
    public Guid TossWonByTeamId { get; set; }
    public string TossDecision { get; set; } = string.Empty; // "Bat" or "Bowl"
    
    // Full Playing XI Squad Arrays (Stored cleanly as database arrays/JSONB)
    public List<Guid> TeamAPlayers { get; set; } = new();
    public List<Guid> TeamBPlayers { get; set; } = new();
    
    public DateTime ScheduledStartUtc { get; set; }
}
