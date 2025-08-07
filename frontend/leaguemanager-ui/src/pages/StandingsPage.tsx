import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { TeamStats } from '../types';

// This function will fetch the data from our API
const fetchStandings = async (): Promise<TeamStats[]> => {
  const { data } = await apiClient.get('/leaguetable');
  return data;
};

export default function StandingsPage() {
  // The useQuery hook handles everything for us
  const { data: standings, isLoading, isError, error } = useQuery({
    queryKey: ['standings'], // A unique key for this query
    queryFn: fetchStandings, // The function that will fetch the data
  });

  if (isLoading) {
    return <div>Loading standings...</div>;
  }

  if (isError) {
    return <div>Error fetching standings: {error.message}</div>;
  }

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">League Standings</h1>
      <table className="min-w-full bg-sky-600 border">
        <thead>
          <tr className="bg-sky-400">
            <th className="py-2 px-4 border-b">Team</th>
            <th className="py-2 px-4 border-b">P</th>
            <th className="py-2 px-4 border-b">W</th>
            <th className="py-2 px-4 border-b">D</th>
            <th className="py-2 px-4 border-b">L</th>
            <th className="py-2 px-4 border-b">GF</th>
            <th className="py-2 px-4 border-b">GA</th>
            <th className="py-2 px-4 border-b">GD</th>
            <th className="py-2 px-4 border-b">Pts</th>
          </tr>
        </thead>
        <tbody>
          {standings?.map((team) => (
            <tr key={team.teamName} className="text-center">
              <td className="py-2 px-4 border-b text-left">{team.teamName}</td>
              <td className="py-2 px-4 border-b">{team.played}</td>
              <td className="py-2 px-4 border-b">{team.won}</td>
              <td className="py-2 px-4 border-b">{team.drawn}</td>
              <td className="py-2 px-4 border-b">{team.lost}</td>
              <td className="py-2 px-4 border-b">{team.goalsFor}</td>
              <td className="py-2 px-4 border-b">{team.goalsAgainst}</td>
              <td className="py-2 px-4 border-b">{team.goalDifference}</td>
              <td className="py-2 px-4 border-b font-bold">{team.points}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}