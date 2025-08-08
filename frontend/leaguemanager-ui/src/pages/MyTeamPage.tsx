import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { MyTeamResponse, PlayerResponseDto, Team } from '../types';
import { Link } from 'react-router-dom';
import RosterManagement from '../features/teams/RosterManagement';
import EditTeamForm from '../features/teams/EditTeamForm';
import JoinRequestsList from '../features/teams/JoinRequestsList';

// Fetch function for the user's team
const fetchMyTeam = async (): Promise<MyTeamResponse | null> => {
  try {
    const { data } = await apiClient.get('/my-team');
    return data;
  } catch (error) {
    // A 404 from this endpoint is an expected outcome, meaning the user is not on a team.
    return null;
  }
};

// Fetch function for the team's roster
const fetchRoster = async (teamId: number): Promise<PlayerResponseDto[]> => {
  const { data } = await apiClient.get(`/teams/${teamId}/players`);
  return data;
};

// A component for the onboarding checklist, shown for pending teams
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
  // First query: get the user's team and their role on it
  const { data: myTeamData, isLoading: isTeamLoading } = useQuery({
    queryKey: ['myTeam'],
    queryFn: fetchMyTeam,
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

  // This is the "lobby" view for users without a team
  if (!myTeamData) {
    return (
      <div className="container mx-auto p-4 text-center">
        <h2 className="text-2xl font-bold">You are not on a team yet.</h2>
        <p className="text-gray-600 mt-2">
          You can create your own team or request to join an existing one.
        </p>
        <div className="flex justify-center space-x-4 mt-6">
          <Link to="/create-team" className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-semibold">
            Create a Team
          </Link>
          <Link to="/join-team" className="px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 font-semibold">
            Join a Team
          </Link>
        </div>
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
      
      {team.status === 'PendingApproval' && (
        <OnboardingChecklist team={team} playerCount={roster?.length || 0} />
      )}

      {isManager && (
        <div className="mt-6">
          <div className="flex justify-between items-center border-b pb-2 mb-4">
            <h2 className="text-2xl font-semibold">Team Management</h2>
            <Link to="/my-team/fixtures" className="text-blue-600 hover:underline">
              View All Fixtures →
            </Link>
          </div>
          <JoinRequestsList />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mt-4">
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