import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { PlayerResponseDto, Team } from '../types'; 
import { useAuth } from '../contexts/AuthContext';
import TeamManagement from '../features/teams/TeamManagement';

const fetchTeamDetails = async (teamId: string): Promise<Team> => {
  const { data } = await apiClient.get(`/teams/${teamId}`);
  return data;
};

// Add the fetch function for the roster
const fetchRoster = async (teamId: string): Promise<PlayerResponseDto[]> => {
  const { data } = await apiClient.get(`/teams/${teamId}/players`);
  return data;
};

export default function TeamDetailsPage() {
  const { teamId } = useParams<{ teamId: string }>();
  const { isAuthenticated } = useAuth();

  // Query for the team details
  const { data: team, isLoading: isTeamLoading, isError } = useQuery({
    queryKey: ['team', teamId],
    queryFn: () => fetchTeamDetails(teamId!),
    enabled: !!teamId,
  });

  // Add a second query to fetch the roster, dependent on the first
  const { data: roster, isLoading: isRosterLoading } = useQuery({
    queryKey: ['roster', teamId],
    queryFn: () => fetchRoster(teamId!),
    enabled: !!teamId, // Only runs if teamId is available
  });

  if (isTeamLoading) {
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
      
      {/* Pass the roster data down to the management component */}
      {isAuthenticated && (
        <TeamManagement 
          team={team} 
          roster={roster || []} 
          isRosterLoading={isRosterLoading} 
        />
      )}
    </div>
  );
}