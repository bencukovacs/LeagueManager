export interface TeamStats {
  teamName: string;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
}

export interface Team {
  id: number;
  name: string;
  primaryColor: string | null;
  secondaryColor: string | null;
  status: string;
}

export interface Result {
  id: number;
  fixtureId: number;
  homeScore: number;
  awayScore: number;
  status: string;
}