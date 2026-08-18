namespace Application.Matches.Commands.MatchCommands;

public sealed record ClaimScorerCommand(Guid MatchId, int ScorerId);
public sealed record ChangeScorerCommand(Guid MatchId, int ScorerId);
public sealed record RecordTossCommand(Guid MatchId, Guid TossWonTeamId, InningsDecision Decision);
public sealed record StartMatchCommand(Guid MatchId);
public sealed record StartInningsCommand(Guid MatchId, int StrikerId, int NonStrikerId, int BowlerId);
public sealed record RecordDeliveryCommand(Guid MatchId, Delivery delivery);
public sealed record UndoDeliveryCommand(Guid MatchId);