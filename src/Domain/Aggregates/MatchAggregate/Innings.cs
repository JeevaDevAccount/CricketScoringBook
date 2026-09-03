using Domain.Enum;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Innings
{
    public Guid Id { get; private set; }
    public int InningsNumber { get; private set; }
    public int BattingTeamId { get; private set; }
    public int BowlingTeamId { get; private set; }
    public InningsType Type { get; private set; }
    public int MaxOvers { get; private set; }
    public int? TargetRuns { get; private set; }
    public int TotalRuns { get; private set; }
    public int Wickets { get; private set; }
    public int TotalBalls { get; private set; }

    public int CurrentStrikerId { get; private set; }
    public int CurrentNonStrikerId { get; private set; }
    public int CurrentBowlerId { get; private set; }

    public bool IsCompleted { get; private set; }

    public int SuperOverNumber { get; private set; }

    private readonly List<Over> _overs = [];

    public IReadOnlyCollection<Over> Overs =>
        _overs.AsReadOnly();

    private Over CurrentOver =>
        _overs[^1];

    private readonly List<BattingScore> _battingScores = [];
    private readonly List<BowlingScore> _bowlingScores = [];

    public IReadOnlyCollection<BattingScore> BattingScores => _battingScores.AsReadOnly();

    public IReadOnlyCollection<BowlingScore> BowlingScores => _bowlingScores.AsReadOnly();

    private Innings()
    {
    }

    private Innings(
        int inningsNumber,
        int battingTeamId,
        int bowlingTeamId,
        InningsType type,
        int superOverNumber,
        int maxOvers,
        int? targetRuns,
        int currentStrikerId,
        int currentNonStrikerId,
        int currentBowlerId)
    {
        if (inningsNumber <= 0)
            throw new ArgumentException(
                "Innings number must be greater than zero.",
                nameof(inningsNumber));

        if (battingTeamId <= 0)
            throw new ArgumentException(
                "Invalid batting team.",
                nameof(battingTeamId));

        if (bowlingTeamId <= 0)
            throw new ArgumentException(
                "Invalid bowling team.",
                nameof(bowlingTeamId));

        if (battingTeamId == bowlingTeamId)
            throw new ArgumentException(
                "Batting and bowling team cannot be the same.");

        if (currentStrikerId <= 0)
            throw new ArgumentException(
                "Invalid striker.",
                nameof(currentStrikerId));

        if (currentNonStrikerId <= 0)
            throw new ArgumentException(
                "Invalid non-striker.",
                nameof(currentNonStrikerId));

        if (currentBowlerId <= 0)
            throw new ArgumentException(
                "Invalid bowler.",
                nameof(currentBowlerId));

        if (currentStrikerId == currentNonStrikerId)
            throw new ArgumentException(
                "Striker and non-striker cannot be the same.");

        if (!System.Enum.IsDefined(typeof(InningsType), type))
            throw new ArgumentException(
                "Invalid innings type.",
                nameof(type));

        if (maxOvers <= 0)
            throw new ArgumentException(
                "Maximum overs must be greater than zero.",
                nameof(maxOvers));

        if (targetRuns.HasValue && targetRuns.Value <= 0)
            throw new ArgumentException(
                "Target runs must be greater than zero.",
                nameof(targetRuns));

        if (superOverNumber < 0)
            throw new ArgumentException(
                "Super over number cannot be negative.",
                nameof(superOverNumber));

        Id = Guid.NewGuid();

        InningsNumber = inningsNumber;
        BattingTeamId = battingTeamId;
        BowlingTeamId = bowlingTeamId;
        Type = type;
        SuperOverNumber = superOverNumber;
        MaxOvers = maxOvers;
        TargetRuns = targetRuns;

        CurrentStrikerId = currentStrikerId;
        CurrentNonStrikerId = currentNonStrikerId;
        CurrentBowlerId = currentBowlerId;

        _overs.Add(Over.Create(1));
    }

    public static Innings Create(
        int inningsNumber,
        int battingTeamId,
        int bowlingTeamId,
        InningsType type,
        int superOverNumber,
        int maxOvers,
        int? targetRuns,
        int currentStrikerId,
        int currentNonStrikerId,
        int currentBowlerId)
    {
        return new Innings(
            inningsNumber,
            battingTeamId,
            bowlingTeamId,
            type,
            superOverNumber,
            maxOvers,
            targetRuns,
            currentStrikerId,
            currentNonStrikerId,
            currentBowlerId);
    }

    public void RecordDelivery(Match.DeliveryInput input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (IsCompleted)
            throw new InvalidOperationException(
                "The innings has already been completed.");

        ValidateDelivery(input);

        Delivery delivery = CurrentOver.AddDelivery(input);

        BattingScore battingScore =
            GetOrCreateBattingScore(delivery.StrikerId);

        BowlingScore bowlingScore =
            GetOrCreateBowlingScore(delivery.BowlerId);

        battingScore.RecordDelivery(
            delivery.BatterRuns,
            delivery.IsLegal,
            delivery.Dismissal);

        bool creditedWithWicket =
            IsWicketCreditedToBowler(delivery);

        int bowlerRunsConceded =
            delivery.BatterRuns +
            BowlerExtraRuns(delivery);

        bowlingScore.RecordDelivery(
            bowlerRunsConceded,
            delivery.IsLegal,
            creditedWithWicket);

        UpdateScore(delivery);

        RotateStrikeForRuns(delivery);

        if (CurrentOver.IsCompleted)
        {
            RotateStrikeAtEndOfOver();

            if (CurrentOver.IsMaiden)
                bowlingScore.RecordMaiden();

            CheckInningsCompletion();

            if (!IsCompleted)
                CreateNextOver();
        }
        else
        {
            CheckInningsCompletion();
        }
    }

    public void UndoLastDelivery()
    {
        RemoveEmptyCurrentOver();

        Over overToUndo = GetOverToUndo();

        bool wasOverCompleted = overToUndo.IsCompleted;
        bool wasMaiden = overToUndo.IsMaiden;

        Delivery removedDelivery = overToUndo.UndoLastDelivery();

        if (wasOverCompleted)
            RotateStrikeAtEndOfOver();

        RotateStrikeForRuns(removedDelivery);

        UndoScore(removedDelivery);

        BattingScore battingScore =
            GetOrCreateBattingScore(removedDelivery.StrikerId);

        BowlingScore bowlingScore =
            GetOrCreateBowlingScore(removedDelivery.BowlerId);

        battingScore.UndoDelivery(
            removedDelivery.BatterRuns,
            removedDelivery.IsLegal,
            removedDelivery.Dismissal);

        int bowlerRunsConceded =
            removedDelivery.BatterRuns +
            BowlerExtraRuns(removedDelivery);

        bool creditedWithWicket =
            IsWicketCreditedToBowler(removedDelivery);

        bowlingScore.UndoDelivery(
            bowlerRunsConceded,
            removedDelivery.IsLegal,
            creditedWithWicket);

        if (wasMaiden)
            bowlingScore.UndoMaiden();

        UndoDismissedBatter(removedDelivery);

        IsCompleted = false;
    }

    private Over GetOverToUndo()
    {
        if (CurrentOver.Deliveries.Any())
        {
            if (!CurrentOver.IsEditable)
                throw new InvalidOperationException(
                    "Current over is locked for editing.");

            return CurrentOver;
        }

        if (_overs.Count < 2)
            throw new InvalidOperationException(
                "No delivery available to undo.");

        Over previousOver = _overs[^2];

        if (!previousOver.IsEditable)
            throw new InvalidOperationException(
                "Previous over is locked for editing.");

        return previousOver;
    }

    private void UpdateScore(Delivery delivery)
    {
        TotalRuns += delivery.TotalRuns;

        if (delivery.IsWicket)
            Wickets++;

        if (delivery.IsLegal)
            TotalBalls++;
    }

    private void UndoScore(Delivery delivery)
    {
        TotalRuns -= delivery.TotalRuns;

        if (delivery.IsWicket)
            Wickets--;

        if (delivery.IsLegal)
            TotalBalls--;
    }

    private void RotateStrikeAtEndOfOver()
    {
        SwapStrike();
    }

    private void RotateStrikeForRuns(Delivery delivery)
    {
        if (delivery.BatterRuns % 2 != 0 ||
            (delivery.BatterRuns == 0 &&
             delivery.TotalRuns % 2 != 0))
        {
            SwapStrike();
        }
    }

    private void SwapStrike()
    {
        int temp = CurrentStrikerId;
        CurrentStrikerId = CurrentNonStrikerId;
        CurrentNonStrikerId = temp;
    }

    public void ChangeBowler(int bowlerId)
    {
        if (bowlerId <= 0)
            throw new ArgumentException(
                "Invalid bowler.",
                nameof(bowlerId));

        CurrentBowlerId = bowlerId;
    }

    private void CreateNextOver()
    {
        _overs.Add(Over.Create(_overs.Count + 1));
    }

    private void RemoveEmptyCurrentOver()
    {
        if (_overs.Count <= 1)
            return;

        if (!CurrentOver.Deliveries.Any())
            _overs.RemoveAt(_overs.Count - 1);
    }

    private void ValidateDelivery(Match.DeliveryInput input)
    {
        if (input.StrikerId != CurrentStrikerId)
            throw new InvalidOperationException(
                "Delivery striker does not match the current striker.");

        if (input.NonStrikerId != CurrentNonStrikerId)
            throw new InvalidOperationException(
                "Delivery non-striker does not match the current non-striker.");

        if (input.BowlerId != CurrentBowlerId)
            throw new InvalidOperationException(
                "Delivery bowler does not match the current bowler.");
    }

    private BattingScore GetOrCreateBattingScore(int playerId)
    {
        BattingScore? score = _battingScores.FirstOrDefault(x => x.PlayerId == playerId);

        if (score is not null)
            return score;

        score = BattingScore.Create(playerId);
        _battingScores.Add(score);

        return score;
    }

    private BowlingScore GetOrCreateBowlingScore(int playerId)
    {
        BowlingScore? score = _bowlingScores.FirstOrDefault(x => x.PlayerId == playerId);

        if (score is not null)
            return score;

        score = BowlingScore.Create(playerId);
        _bowlingScores.Add(score);

        return score;
    }

    private static int BowlerExtraRuns(Delivery delivery)
    {
        return delivery.ExtraType switch
        {
            ExtraType.Wide => 1,
            ExtraType.NoBall => 1,
            _ => 0
        };
    }

    private static bool IsWicketCreditedToBowler(Delivery delivery)
    {
        if (delivery.Dismissal is null)
            return false;

        return delivery.Dismissal.WicketType switch
        {
            WicketType.None => false,
            WicketType.RunOut => false,
            _ => true
        };
    }

    public void ReplaceDismissedBatter(
        int dismissedPlayerId,
        int incomingBatterId)
    {
        if (dismissedPlayerId <= 0)
            throw new ArgumentException(
                "Invalid dismissed player.",
                nameof(dismissedPlayerId));

        if (incomingBatterId <= 0)
            throw new ArgumentException(
                "Invalid incoming batter.",
                nameof(incomingBatterId));

        if (dismissedPlayerId == incomingBatterId)
            throw new ArgumentException(
                "Incoming batter cannot be the dismissed batter.",
                nameof(incomingBatterId));

        if (CurrentStrikerId == incomingBatterId ||
            CurrentNonStrikerId == incomingBatterId)
        {
            throw new InvalidOperationException(
                "Incoming batter is already on the field.");
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

        if (!delivery.DismissedPlayerId.HasValue)
            throw new InvalidOperationException(
                "Dismissed player is missing for a wicket.");

        int dismissedPlayerId =
            delivery.DismissedPlayerId.Value;

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

        throw new InvalidOperationException(
            "Unable to restore dismissed batter.");
    }

    private void CheckInningsCompletion()
    {
        if (Wickets >= 10)
        {
            IsCompleted = true;
            return;
        }

        if (TargetRuns.HasValue &&
            TotalRuns >= TargetRuns.Value)
        {
            IsCompleted = true;
            return;
        }

        if (TotalBalls >= MaxOvers * 6)
            IsCompleted = true;
    }
}