using Domain.Enum;
using Domain.Aggregates.MatchAggregate;

namespace Application.Matches.Commands.MatchCommands;

public sealed record ClaimScorerCommand(Guid MatchId, int ScorerId);
public sealed record ChangeScorerCommand(Guid MatchId, int ScorerId);
public sealed record RecordTossCommand(Guid MatchId, int TossWonTeamId, InningsDecision Decision);
public sealed record CreateMatchCommand(int Team1Id, int Team2Id, int MaxOvers);
public sealed record StartMatchCommand(Guid MatchId);
public sealed record StartInningsCommand(Guid MatchId, int StrikerId, int NonStrikerId, int BowlerId);

public sealed record RecordDeliveryCommand(
    Guid MatchId,
    int StrikerId,
    int NonStrikerId,
    int BowlerId,
    int BatterRuns,
    int TotalRuns,
    ExtraType ExtraType,
    WicketType WicketType,
    int? FielderId,
    int? DismissedPlayerId,
    int? IncomingBatterId);

public sealed record UndoDeliveryCommand(Guid MatchId);

public sealed record AddPlayerToPlayingTeamCommand(Guid MatchId, int TeamId, int PlayerId);