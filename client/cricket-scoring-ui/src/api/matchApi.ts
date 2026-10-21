import type { MatchDto } from "../types/match";

const API_BASE_URL = "http://localhost:5000/api";

export async function getMatches(): Promise<MatchDto[]> {
  const response = await fetch(`${API_BASE_URL}/matches/getAllMatches`);

  if (!response.ok) {
    throw new Error("Failed to load matches.");
  }

  return response.json();
}