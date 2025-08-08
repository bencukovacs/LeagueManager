import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { PlayerResponseDto } from '../types';
import { useState } from 'react';
import AssignPlayerModal from '../features/admin/AssignPlayerModal';

// API Functions
const fetchUnassignedPlayers = async (): Promise<PlayerResponseDto[]> => {
  const { data } = await apiClient.get('/players/unassigned');
  return data;
};

const deletePlayer = async (playerId: number) => {
  await apiClient.delete(`/players/${playerId}`);
};

export default function AdminManagePlayersPage() {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPlayer, setSelectedPlayer] = useState<PlayerResponseDto | null>(null);

  const { data: players, isLoading } = useQuery({
    queryKey: ['unassignedPlayers'],
    queryFn: fetchUnassignedPlayers,
  });

  const deleteMutation = useMutation({
    mutationFn: deletePlayer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['unassignedPlayers'] });
    },
  });

  const handleAssign = (player: PlayerResponseDto) => {
    setSelectedPlayer(player);
    setIsModalOpen(true);
  };

  const handleDelete = (playerId: number) => {
    if (window.confirm('Are you sure you want to permanently delete this player? This cannot be undone.')) {
      deleteMutation.mutate(playerId);
    }
  };

  return (
    <>
      <h2 className="text-2xl font-semibold mb-4">Manage Unassigned Players</h2>
      {isLoading ? (
        <p>Loading players...</p>
      ) : (
        <div className="space-y-2">
          {players?.map((player) => (
            <div key={player.id} className="p-3 bg-sky-400 border rounded-lg shadow-sm flex justify-between items-center">
              <p className="font-semibold">{player.name}</p>
              <div className="flex space-x-2">
                <button onClick={() => handleAssign(player)} className="text-sm text-blue-600 hover:underline">Assign to Team</button>
                <button onClick={() => handleDelete(player.id)} className="text-sm text-red-600 hover:underline">Delete Permanently</button>
              </div>
            </div>
          ))}
        </div>
      )}

      <AssignPlayerModal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        player={selectedPlayer} 
      />
    </>
  );
}