import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { Team } from '../types';

// This function fetches the data for a single team
const fetchTeamDetails = async (teamId: string): Promise<Team> => {
  const { data } = await apiClient.get(`/teams/${teamId}`);
  return data;
};

export default function TeamDetailsPage() {
  const { teamId } = useParams<{ teamId: string }>();

  const { data: team, isLoading, isError } = useQuery({
    queryKey: ['team', teamId],
    queryFn: () => fetchTeamDetails(teamId!),
    enabled: !!teamId, // Only run the query if teamId is available
  });

  if (isLoading) {
    return <div className="p-4">Loading team details...</div>;
  }

  if (isError || !team) {
    return <div className="p-4 text-red-500">Error fetching team details.</div>;
  }

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold">{team.name}</h1>
      <p className={`mt-2 text-sm font-semibold ${team.status === 'Approved' ? 'text-green-600' : 'text-yellow-600'}`}>
        Status: {team.status}
      </p>
      <div className="mt-4 space-y-2">
        <p><strong>Primary Color:</strong> {team.primaryColor || 'Not set'}</p>
        <p><strong>Secondary Color:</strong> {team.secondaryColor || 'Not set'}</p>
      </div>
      {/* We will add the player roster here later */}
    </div>
  );
}