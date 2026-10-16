export type MatchStatus =
  | "Created"
  | "Started"
  | "InProgress"
  | "Completed";

export interface MatchDto {
  matchId: string;

  team1Id: number;
  team1Name: string;

  team2Id: number;
  team2Name: string;

  status: MatchStatus;

  teamBattingFirstId: number | null;
  teamBattingSecondId: number | null;
}