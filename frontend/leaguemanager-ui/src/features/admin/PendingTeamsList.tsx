import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { Team } from '../../types';
import { useState } from 'react';


const fetchPendingTeams = async (): Promise<Team[]> => {
  const { data } = await apiClient.get('/teams/pending');
  return data;
};
const approveTeam = async (teamId: number) => {
  await apiClient.patch(`/teams/${teamId}/approve`);
};


export default function PendingTeamsList() {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const { data: pendingTeams, isLoading, isError } = useQuery({
    queryKey: ['pendingTeams'],
    queryFn: fetchPendingTeams,
  });

  const approveMutation = useMutation({
    mutationFn: approveTeam,
    onSuccess: () => {
      setError(null);
      queryClient.invalidateQueries({ queryKey: ['pendingTeams'] });
    },
    onError: (err: any) => {
      if (err.response && err.response.data) {
        setError(err.response.data);
      } else {
        setError('An unexpected error occurred.');
      }
    },
  });

  if (isLoading) return <div>Loading pending teams...</div>;
  if (isError) return <div className="text-red-500">Failed to load pending teams.</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold mb-2">Pending Team Approvals</h2>
      
      {error && <div className="p-2 mb-4 text-sm text-red-700 bg-red-100 rounded-md">{error}</div>}

      {pendingTeams && pendingTeams.length > 0 ? (
        <ul className="space-y-2">
          {pendingTeams.map((team) => (
            <li key={team.id} className="flex justify-between items-center p-2 border rounded">
              <span>{team.name}</span>
              <button
                onClick={() => {
                  setError(null);
                  approveMutation.mutate(team.id);
                }}
                disabled={approveMutation.isPending}
                className="px-3 py-1 bg-green-500 text-white rounded hover:bg-green-600 disabled:bg-gray-400"
              >
                Approve
              </button>
            </li>
          ))}
        </ul>
      ) : (
        <p>No teams are currently waiting for approval.</p>
      )}
    </div>
  );
}