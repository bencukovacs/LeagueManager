import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { MyTeamResponse, PlayerResponseDto, Team } from '../types';
import { Link } from 'react-router-dom';
import RosterManagement from '../features/teams/RosterManagement';
import EditTeamForm from '../features/teams/EditTeamForm';

const fetchMyTeam = async (): Promise<MyTeamResponse> => {
  const { data } = await apiClient.get('/my-team');
  return data;
};

const fetchRoster = async (teamId: number): Promise<PlayerResponseDto[]> => {
  const { data } = await apiClient.get(`/teams/${teamId}/players`);
  return data;
};

// A new component for the onboarding checklist
const OnboardingChecklist = ({ team, playerCount }: { team: Team, playerCount: number }) => {
  const minPlayers = 5; // This will later come from the LeagueConfiguration

  const hasEnoughPlayers = playerCount >= minPlayers;
  const hasSetColors = !!team.primaryColor;

  return (
    <div className="p-4 mb-6 border-l-4 border-yellow-400 bg-yellow-50">
      <h3 className="text-lg font-semibold text-yellow-800">Your team is awaiting admin approval.</h3>
      <p className="text-yellow-700">Please complete the following steps:</p>
      <ul className="list-disc list-inside mt-2 space-y-1">
        <li className={hasSetColors ? 'text-green-600' : 'text-gray-700'}>
          {hasSetColors ? '✓' : '•'} Set your team colors.
        </li>
        <li className={hasEnoughPlayers ? 'text-green-600' : 'text-gray-700'}>
          {hasEnoughPlayers ? '✓' : '•'} Add at least {minPlayers} players to your roster (Current: {playerCount}).
        </li>
      </ul>
    </div>
  );
};

export default function MyTeamPage() {
  // First query: get the user's team
  const { data: myTeamData, isLoading: isTeamLoading, isError: isTeamError } = useQuery({
    queryKey: ['myTeam'],
    queryFn: fetchMyTeam,
    retry: false,
  });

  const teamId = myTeamData?.team?.id;

  // Second query: get the roster, which depends on the first query
  const { data: roster, isLoading: isRosterLoading } = useQuery({
    queryKey: ['roster', teamId],
    queryFn: () => fetchRoster(teamId!),
    enabled: !!teamId,
  });

  if (isTeamLoading) {
    return <div className="p-4">Loading your team...</div>;
  }

  if (isTeamError || !myTeamData) {
    return (
      <div className="container mx-auto p-4 text-center">
        <h2 className="text-xl font-semibold text-red-600">Could not load team</h2>
        <p className="text-gray-600 mt-2">
          You do not currently manage a team.
        </p>
        <Link to="/create-team" className="mt-4 inline-block px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
          Create a Team
        </Link>
      </div>
    );
  }

  const { team, userRole } = myTeamData;
  const isManager = userRole === 'Leader' || userRole === 'AssistantLeader';

  return (
    <div className="container mx-auto p-4">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h1 className="text-3xl font-bold">{team.name}</h1>
          <p className={`mt-2 text-sm font-semibold ${team.status === 'Approved' ? 'text-green-600' : 'text-yellow-600'}`}>
            Status: {team.status}
          </p>
          <p className="text-sm text-gray-500">Your Role: {userRole}</p>
        </div>
      </div>
      
      {/* Conditionally render the checklist only if the team is pending */}
      {team.status === 'PendingApproval' && (
        <OnboardingChecklist team={team} playerCount={roster?.length || 0} />
      )}

      {isManager && (
        <div className="mt-6">
          <h2 className="text-2xl font-semibold border-b pb-2 mb-4">Team Management</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Pass the roster data and loading state down as props */}
            <RosterManagement teamId={team.id} roster={roster || []} isLoading={isRosterLoading} />
            <EditTeamForm team={team} />
          </div>
        </div>
      )}
      
      {!isManager && (
        <div className="mt-6">
          <h2 className="text-2xl font-semibold border-b pb-2 mb-4">My Team</h2>
          <p>Welcome, {team.name} team member!</p>
          {/* We can display the roster here for viewing later */}
        </div>
      )}
    </div>
  );
}