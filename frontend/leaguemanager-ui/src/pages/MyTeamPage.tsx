import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { MyTeamAndConfigResponse, PlayerResponseDto, RosterRequestResponseDto, Team } from '../types';
import { Link } from 'react-router-dom';
import RosterManagement from '../features/teams/RosterManagement';
import EditTeamForm from '../features/teams/EditTeamForm';
import JoinRequestsList from '../features/teams/JoinRequestsList';
import MyPendingRequests from '../features/me/MyPendingRequests';
import { useAuth } from '../contexts/AuthContext';
import LeaveTeamButton from "../features/teams/LeaveTeamButton";
import DisbandTeamButton from '../features/teams/DisbandTeamButton';
import { AxiosError } from 'axios';

// This fetch function now expects the new, combined response object
const fetchMyTeamAndConfig = async (): Promise<MyTeamAndConfigResponse | null> => {
  try {
    const { data } = await apiClient.get('/my-team');
    return data;
  } catch (err) { // 2. The error is now typed
      const error = err as AxiosError;
      if (error.response?.status === 404) {
          return null;
      }
      throw error;
  }
};

// Fetch function for the team's roster
const fetchRoster = async (teamId: number): Promise<PlayerResponseDto[]> => {
  const { data } = await apiClient.get(`/teams/${teamId}/players`);
  return data;
};

// Fetch function for the user's pending requests
const fetchMyPendingRequests = async (): Promise<RosterRequestResponseDto[]> => {
    const { data } = await apiClient.get('/me/roster-requests');
    return data;
};

// The checklist now gets the minPlayers value from its props
const OnboardingChecklist = ({ team, playerCount, minPlayers }: { team: Team, playerCount: number, minPlayers: number }) => {
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
  const { user } = useAuth();
  
  // This query now fetches the combined team and config data
  const { data: response, isLoading: isTeamLoading } = useQuery({
    queryKey: ['myTeamAndConfig'],
    queryFn: fetchMyTeamAndConfig,
  });

  const myTeamData = response?.myTeam;
  const config = response?.config;
  const teamId = myTeamData?.team?.id;

  // Query to fetch the user's pending requests, runs only if they are not on a team
  const { data: pendingRequests } = useQuery({
    queryKey: ['myPendingRequests'],
    queryFn: fetchMyPendingRequests,
    enabled: !myTeamData,
  });

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
    const hasPendingRequest = pendingRequests && pendingRequests.length > 0;
    return (
      <div className="container mx-auto p-4 text-center">
        <h2 className="text-2xl font-bold">You are not on a team yet.</h2>
        <p className="text-gray-600 mt-2">
          You can create your own team or request to join an existing one.
        </p>
        <div className="flex justify-center space-x-4 mt-6">
          <Link
            to="/create-team"
            className={`px-6 py-3 rounded-lg font-semibold ${hasPendingRequest ? 'bg-gray-400 text-gray-700 cursor-not-allowed' : 'bg-indigo-600 text-white hover:bg-indigo-700'}`}
            onClick={(e) => { if (hasPendingRequest) e.preventDefault(); }}
          >
            Create a Team
          </Link>
          <Link
            to="/join-team"
            className={`px-6 py-3 rounded-lg font-semibold ${hasPendingRequest ? 'bg-gray-400 text-gray-700 cursor-not-allowed' : 'bg-green-600 text-white hover:bg-green-700'}`}
            onClick={(e) => { if (hasPendingRequest) e.preventDefault(); }}
          >
            Join a Team
          </Link>
        </div>
        <MyPendingRequests />
      </div>
    );
  }

  const { team, userRole } = myTeamData;
  const isLeader = userRole === 'Leader';
  const isManager = userRole === 'Leader' || userRole === 'AssistantLeader';
  const isAdmin = user?.roles.includes('Admin') ?? false;

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
          <div className="flex space-x-2">
              {isLeader && team.status === 'Approved' && <DisbandTeamButton />}
              <LeaveTeamButton teamStatus={team.status as 'PendingApproval' | 'Approved'} />
          </div>
      </div>
      
      {/* The checklist now gets the minPlayers value from the fetched config */}
      {team.status === 'PendingApproval' && config && (
        <OnboardingChecklist 
          team={team} 
          playerCount={roster?.length || 0} 
          minPlayers={config.minPlayersPerTeam} 
        />
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
            <RosterManagement 
              teamId={team.id} 
              roster={roster || []} 
              isLoading={isRosterLoading}
              isAdmin={isAdmin}
              currentUserRole={userRole}
            />
            <EditTeamForm team={team} />
          </div>
        </div>
      )}
      
      {!isManager && (
        <div className="mt-6">
          <h2 className="text-2xl font-semibold border-b pb-2 mb-4">My Team</h2>
          <p>Welcome, {team.name} team member!</p>
        </div>
      )}
    </div>
  );
}