import type { MatchDto } from "../../types/match";

interface MatchListProps {
  matches: MatchDto[];
}

export function MatchList({ matches }: MatchListProps) {
  if (matches.length === 0) {
    return <p>No matches available.</p>;
  }

  return (
    <div>
      {matches.map((match) => (
        <div key={match.matchId}>
          <h3>
            {match.team1Name} vs {match.team2Name}
          </h3>

          <p>Status: {match.status}</p>
        </div>
      ))}
    </div>
  );
}