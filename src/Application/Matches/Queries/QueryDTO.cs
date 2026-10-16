using Domain.Aggregates.MatchAggregate;
using Domain.Enum;

namespace Application.Matches.Queries;

public sealed record MatchDto(
    Guid MatchId,
    int Team1Id,
    string Team1Name,
    int Team2Id,
    string Team2Name,
    MatchStatus Status,
    int? TeamBattingFirstId,
    int? TeamBattingSecondId
);

public sealed record LiveMatchDto(
    Guid MatchId,
    MatchStatus Status,
    int BattingTeamId,
    string BattingTeamName,
    int BowlingTeamId,
    string BowlingTeamName,
    int TotalRuns,
    int? TargetRuns,
    int Wickets,
    int TotalBalls,
    int StrikerId,
    string StrikerName,
    int NonStrikerId,
    string NonStrikerName,
    int BowlerId,
    string BowlerName,
    int CurrentOverNumber
);

public sealed record ScorecardDto(
    Guid MatchId,
    IReadOnlyList<InningsScorecardDto> Innings
);

public sealed record InningsScorecardDto(
    int InningsNumber,
    int BattingTeamId,
    string BattingTeamName,
    int BowlingTeamId,
    string BowlingTeamName,
    int TotalRuns,
    int Wickets,
    int TotalBalls,
    IReadOnlyList<BattingScoreDto> BattingScores,
    IReadOnlyList<BowlingScoreDto> BowlingScores
);

public sealed record BattingScoreDto(
    int PlayerId,
    string PlayerName,
    int Runs,
    int Balls,
    int Fours,
    int Sixes,
    decimal StrikeRate,
    Dismissal? DismissalType
);

public sealed record BowlingScoreDto(
    int PlayerId,
    string PlayerName,
    int RunsConceded,
    int Balls,
    int Maidens,
    int TotalWickets,
    decimal Economy
);

public sealed record PlayerLookupDto(
    int PlayerId,
    string Name
);