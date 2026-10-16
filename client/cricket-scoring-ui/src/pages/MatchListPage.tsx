import { useEffect, useState } from "react";
import { getMatches } from "../api/matchApi";
import { MatchList } from "../components/MatchList/MatchList";
import type { MatchDto } from "../types/match";

export function MatchListPage() {
  const [matches, setMatches] = useState<MatchDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadMatches() {
      try {
        const data = await getMatches();
        setMatches(data);
      } catch {
        setError("Unable to load matches.");
      } finally {
        setIsLoading(false);
      }
    }

    loadMatches();
  }, []);

  if (isLoading) {
    return <p>Loading matches...</p>;
  }

  if (error) {
    return <p>{error}</p>;
  }

  return (
    <main>
      <h1>Matches</h1>

      <MatchList matches={matches} />
    </main>
  );
}