import { useQuery, useMutation } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { Team } from '../types';
import { useState } from 'react';

// Function to fetch all approved teams
const fetchTeams = async (): Promise<Team[]> => {
  const { data } = await apiClient.get('/teams');
  return data;
};

// Function to send a join request
const createJoinRequest = async (teamId: number) => {
  await apiClient.post(`/roster-requests/join/${teamId}`);
};

export default function JoinTeamPage() {
  const [requestStatus, setRequestStatus] = useState<{ [key: number]: string }>({});

  const { data: teams, isLoading, isError } = useQuery({
    queryKey: ['approvedTeams'],
    queryFn: fetchTeams,
  });

  const mutation = useMutation({
    mutationFn: createJoinRequest,
    onSuccess: (_, teamId) => {
      setRequestStatus(prev => ({ ...prev, [teamId]: 'Request Sent!' }));
    },
    onError: (err: any, teamId) => {
      setRequestStatus(prev => ({ ...prev, [teamId]: err.response?.data || 'Failed to send request.' }));
    },
  });

  if (isLoading) return <div className="p-4">Loading teams...</div>;
  if (isError) return <div className="p-4 text-red-500">Failed to load teams.</div>;

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Join a Team</h1>
      <p className="mb-4 text-gray-600">Below is a list of all approved teams in the league. You can send a request to join a team, which will be sent to the Team Leader for approval.</p>
      
      <div className="space-y-3">
        {teams?.map((team) => (
          <div key={team.id} className="p-4 border rounded-lg bg-white shadow-sm flex justify-between items-center">
            <div>
              <h2 className="text-xl font-bold">{team.name}</h2>
              <div className="flex items-center space-x-2 mt-1">
                <span className="text-sm text-gray-500">Primary Color:</span>
                <div className="w-4 h-4 rounded-full" style={{ backgroundColor: team.primaryColor || 'transparent', border: '1px solid #ccc' }}></div>
              </div>
            </div>
            <div>
              <button
                onClick={() => mutation.mutate(team.id)}
                disabled={mutation.isPending || !!requestStatus[team.id]}
                className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-400"
              >
                {requestStatus[team.id] ? requestStatus[team.id] : 'Request to Join'}
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}