namespace Application.Matches.Queries;

public sealed record MatchDto(
    Guid MatchId,
    int Team1Id,
    int Team2Id,
    MatchStatus Status,
    int? TeamBattingFirstId,
    int? TeamBattingSecondId
);

public sealed record LiveMatchDto(
    Guid MatchId,
    MatchStatus Status,
    int BattingTeamId,
    int BowlingTeamId,
    int TotalRuns,
    int? TargetRuns,
    int Wickets,
    int TotalBalls,
    int StrikerId,
    int NonStrikerId,
    int BowlerId,
    int CurrentOverNumber
);

public sealed record ScorecardDto(
    Guid MatchId,
    IReadOnlyList<InningsScorecardDto> Innings
);

public sealed record InningsScorecardDto(
    int InningsNumber,
    int BattingTeamId,
    int BowlingTeamId,
    int TotalRuns,
    int Wickets,
    int TotalBalls,
    IReadOnlyList<BattingScoreDto> BattingScores,
    IReadOnlyList<BowlingScoreDto> BowlingScores
);

public sealed record BattingScoreDto(
    int PlayerId,
    int Runs,
    int Balls,
    int Fours,
    int Sixes,
    decimal StrikeRate,
    DismissalType DismissalType
);

public sealed record BowlingScoreDto(
    int PlayerId,
    int RunsConceded,
    int Balls,
    int Maidens,
    int TotalWickets,
    decimal Economy
);