using Domain.Enum;
using Domain.Entities;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Match
{
    public Guid Id { get; private set; }
    public MatchStatus Status { get; private set; }
    public string? ActiveScorerId { get; private set; }

    public int MaxOvers { get; private set; }
    public DateTime Timestamp {get; private set;}

    public Guid Team1Id { get; private set; }
    public Guid Team2Id { get; private set; }

    // Toss
    public Guid? TossWonTeamId { get; private set; }
    public Guid? TeamBattingFirstId { get; private set; }
    public Guid? TeamBattingSecondId { get; private set; }

    private readonly List<Innings> _innings = new();
    public IReadOnlyCollection<Innings> Innings => _innings.AsReadOnly();

    private Innings CurrentInnings => _innings[^1];
    public Guid? WinnerTeamId { get; private set; }
    public bool IsTie { get; private set; }

    private Match(){}

    private Match(Guid team1Id, Guid team2Id, int maxOvers){
        if (team1Id == Guid.Empty)
            throw new ArgumentException("Invalid first team.",nameof(team1Id));

        if (team2Id == Guid.Empty)
            throw new ArgumentException("Invalid second team.",nameof(team2Id));

        if (team1Id == team2Id)
            throw new ArgumentException("Both teams cannot be the same.");

        if (maxOvers <= 0)
            throw new ArgumentException("Maximum overs must be greater than zero.",nameof(maxOvers));

        Id = Guid.NewGuid();
        Status = MatchStatus.Scheduled;

        Team1Id = team1Id;
        Team2Id = team2Id;
        MaxOvers = maxOvers;
    }

    public static Match Create(Guid team1Id, Guid team2Id, int maxOvers){
        return new Match(team1Id,team2Id,maxOvers)
    }

    public void ClaimScorer(string scorerId){
        if (string.IsNullOrWhiteSpace(scorerId))
            throw new ArgumentException("Scorer is required.",nameof(scorerId));

        if (ActiveScorerId is not null)
            throw new InvalidOperationException("A scorer is already active for this match.");
        
        ActiveScorerId = scorerId;
    }

    public void ChangeScorer(string activeScorerId, string newScorerId){
        if (string.IsNullOrWhiteSpace(newScorerId))
            throw new ArgumentException("Scorer is required.",nameof(scorerId));

        if (ActiveScorerId != activeScorerId)
            throw new InvalidOperationException("Only the active scorer can assign the new scorer");
        
        if (ActiveScorerId == newScorerId)
            throw new InvalidOperationException("The new scorer is already the active scorer.");

        ActiveScorerId = newScorerId;
    }

    public void Toss(GUID tossWonTeamId, InningsDecision decision){
        if (tossWonTeamId != Team1Id && tossWonTeamId != Team2Id)
            throw new ArgumentException("Toss winning team does not belong to this match.",nameof(tossWonTeamId));

        TossWonTeamId = tossWonTeamId;
        bool team1WonToss = tossWonTeamId == Team1Id;
   
        if (decision == InningsDecision.Bat)
        {
            TeamBattingFirstId = tossWonTeamId;
            TeamBattingSecondId = team1WonToss ? Team2Id : Team1Id;
        }
        else
        {
            TeamBattingSecondId = tossWonTeamId;
            TeamBattingFirstId = team1WonToss ? Team2Id : Team1Id;
        }
    }

    public void StartMatch()
    {
        if (Status != MatchStatus.Scheduled)
            throw new InvalidOperationException(
                "Only a scheduled match can be started.");

        if (string.IsNullOrWhiteSpace(ActiveScorerId))
            throw new InvalidOperationException(
                "A scorer must be assigned before starting the match.");

        if (!TossWonTeamId.HasValue ||
            !TeamBattingFirstId.HasValue ||
            !TeamBattingSecondId.HasValue)
        {
            throw new InvalidOperationException(
                "Toss must be completed before starting the match.");
        }

        Status = MatchStatus.Live;
    }

    public void StartInnings(int strikerId, int nonStrikerId, int bowlerId)
    {
        if (Status != MatchStatus.Live)
            throw new InvalidOperationException("Innings can only be started when the match is live.");

        if (!TeamBattingFirstId.HasValue || !TeamBattingSecondId.HasValue)
        {
            throw new InvalidOperationException("Toss must be completed before starting an innings.");
        }

        if (_innings.Count > 0)
            throw new InvalidOperationException("The first innings has already been started.");

        if (strikerId <= 0)
            throw new ArgumentException("Invalid striker.",nameof(strikerId));

        if (nonStrikerId <= 0)
            throw new ArgumentException("Invalid non-striker.",nameof(nonStrikerId));

        if (bowlerId <= 0)
            throw new ArgumentException("Invalid bowler.",nameof(bowlerId));

        if (strikerId == nonStrikerId)
            throw new ArgumentException("Striker and non-striker cannot be the same.");

        Innings innings = Innings.Create(1,TeamBattingFirstId.Value,TeamBattingSecondId.Value,
            InningsType.Regular,MaxOvers,targetRuns,strikerId,nonStrikerId,bowlerId);
            
        _innings.Add(innings);
    }

    public void StartSecondInnings(int strikerId, int nonStrikerId, int bowlerId)
    {
        if (Status != MatchStatus.InningsBreak)
            throw new InvalidOperationException("Second innings can only start during the innings break.");

        if (_innings.Count != 1)
            throw new InvalidOperationException("First innings must be completed before starting the second innings.");

        if (!TeamBattingFirstId.HasValue || !TeamBattingSecondId.HasValue)
        {
            throw new InvalidOperationException("Toss must be completed before starting the second innings.");
        }

        int targetRuns = _innings[0].TotalRuns + 1;

        Innings innings = Innings.Create(2,TeamBattingSecondId.Value,TeamBattingFirstId.Value,
            InningsType.Regular,MaxOvers,targetRuns, strikerId,nonStrikerId,bowlerId);
        
        _innings.Add(innings);
        Status = MatchStatus.Live;
    }

    public void RecordDelivery(Delivery delivery){
        if (delivery is null)
            throw new ArgumentNullException(nameof(delivery));

        if (Status != MatchStatus.Live)
            throw new InvalidOperationException("Delivery can only be recorded when the match is live.");

        if (_innings.Count == 0)
            throw new InvalidOperationException("An innings has not been started.");

        CurrentInnings.RecordDelivery(delivery)

        if (CurrentInnings.IsCompleted)
        {
            HandleInningsCompleted();
        }
    }

    public void UndoDelivery(){
        if (Status != MatchStatus.Live)
            throw new InvalidOperationException("A delivery can only be undone while the match is live.");

        if (_innings.Count == 0)
            throw new InvalidOperationException("An innings has not been started.");

        CurrentInnings.UndoLastDelivery()
    }

    private void HandleInningsCompleted()
    {
        if (_innings.Count == 1)
        {
            Status = MatchStatus.InningsBreak;
            return;
        }

        DetermineResult();

        Status = MatchStatus.Completed;
    }

    private void DetermineResult()
    {
        Innings firstInnings = _innings[0];
        Innings secondInnings = _innings[1];

        if (secondInnings.TotalRuns > firstInnings.TotalRuns)
        {
            WinnerTeamId = secondInnings.BattingTeamId;
            return;
        }

        if (firstInnings.TotalRuns > secondInnings.TotalRuns)
        {
            WinnerTeamId = firstInnings.BattingTeamId;
            return;
        }

        IsTie = true;
    }
}