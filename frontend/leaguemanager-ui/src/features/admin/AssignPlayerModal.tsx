import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { PlayerResponseDto, Team } from '../../types';

interface AssignPlayerModalProps {
  player: PlayerResponseDto | null;
  isOpen: boolean;
  onClose: () => void;
}

// API Functions
const assignPlayer = async ({ playerId, teamId }: { playerId: number; teamId: number }) => {
  await apiClient.patch(`/players/${playerId}/assign/${teamId}`);
};

const fetchApprovedTeams = async (): Promise<Team[]> => {
    const { data } = await apiClient.get('/teams');
    return data;
};

export default function AssignPlayerModal({ player, isOpen, onClose }: AssignPlayerModalProps) {
  const queryClient = useQueryClient();
  const [selectedTeamId, setSelectedTeamId] = useState<number | undefined>();

  const { data: teams } = useQuery({ queryKey: ['approvedTeams'], queryFn: fetchApprovedTeams });

  const mutation = useMutation({
    mutationFn: assignPlayer,
    onSuccess: () => {
      // Refetch the list of unassigned players to update the UI
      queryClient.invalidateQueries({ queryKey: ['unassignedPlayers'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!player || !selectedTeamId) return;
    mutation.mutate({ playerId: player.id, teamId: selectedTeamId });
  };

  if (!isOpen || !player) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-sky-400 p-6 rounded-lg shadow-xl w-full max-w-md">
        <h2 className="text-2xl font-bold mb-4">Assign Player to Team</h2>
        <p className="mb-4">Assigning <span className="font-semibold">{player.name}</span> to a team.</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label>Select Team</label>
            <select 
              onChange={(e) => setSelectedTeamId(Number(e.target.value))} 
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" 
              required
            >
              <option value="">Select a team</option>
              {teams?.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          {mutation.isError && <p className="text-red-500 text-sm">Failed to assign player.</p>}
          <div className="flex justify-end space-x-2 pt-4">
            <button type="button" onClick={onClose} className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">Cancel</button>
            <button type="submit" disabled={mutation.isPending} className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400">Assign Player</button>
          </div>
        </form>
      </div>
    </div>
  );
}