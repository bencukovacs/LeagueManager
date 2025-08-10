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

export interface PlayerResponseDto {
  id: number;
  name: string;
  teamId: number;
  teamName: string;
}

export interface MyTeamResponse {
  team: Team;
  userRole: 'Leader' | 'AssistantLeader' | 'Member';
}

export interface FixtureResponseDto {
  id: number;
  homeTeam: Team;
  awayTeam: Team;
  kickOffDateTime: string;
  status: string;
  location: {
    id: number;
    name: string;
  } | null;
  result: Result | null;
  homeTeamRoster: PlayerResponseDto[];
  awayTeamRoster: PlayerResponseDto[];
}

export interface RosterRequestResponseDto {
  id: number;
  userName: string;
  teamName: string;
  type: string;
  status: string;
}

export interface Location {
  id: number;
  name: string;
  address: string | null;
  pitchNumber: string | null;
}

export interface LeagueConfiguration {
  minPlayersPerTeam: number;
  matchLengthMinutes: number;
  midSeasonTransferLimit: number;
  rosterLockDate: string | null;
}

export interface MyTeamAndConfigResponse {
  myTeam: MyTeamResponse | null;
  config: LeagueConfiguration;
}