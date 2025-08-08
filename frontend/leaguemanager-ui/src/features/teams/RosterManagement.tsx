import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { PlayerResponseDto } from '../../types';
import { useState } from 'react';

// Add Player
const addPlayer = async ({ teamId, name }: { teamId: number; name: string }) => {
  await apiClient.post('/players', { name, teamId });
};

// "Soft delete" for Team Leaders
const removePlayerFromRoster = async (playerId: number) => {
  await apiClient.patch(`/players/${playerId}/remove-from-roster`);
};

// "Hard delete" for Admins
const deletePlayerPermanently = async (playerId: number) => {
  await apiClient.delete(`/players/${playerId}`);
};

interface RosterManagementProps {
  teamId: number;
  roster: PlayerResponseDto[];
  isLoading: boolean;
  isAdmin: boolean; // New prop
}

export default function RosterManagement({
  teamId,
  roster,
  isLoading,
  isAdmin
}: RosterManagementProps) {
  const queryClient = useQueryClient();
  const [newPlayerName, setNewPlayerName] = useState('');
  const [error, setError] = useState<string | null>(null);

  const addPlayerMutation = useMutation({
    mutationFn: addPlayer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] });
      setNewPlayerName('');
      setError(null);
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to add player.');
    }
  });

  const removePlayerMutation = useMutation({
    mutationFn: removePlayerFromRoster,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] });
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to remove player.');
    }
  });

  const deletePlayerMutation = useMutation({
    mutationFn: deletePlayerPermanently,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] });
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to permanently delete player.');
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newPlayerName.trim()) return;
    addPlayerMutation.mutate({ teamId, name: newPlayerName });
  };

  return (
    <div className="mt-6 p-4 border rounded-lg">
      <h3 className="text-xl font-semibold mb-4">Manage Roster</h3>

      {/* Add Player Form */}
      <form onSubmit={handleSubmit} className="flex gap-2 mb-4">
        <input
          type="text"
          value={newPlayerName}
          onChange={(e) => setNewPlayerName(e.target.value)}
          placeholder="New player name"
          className="flex-grow px-3 py-2 border border-gray-300 rounded-md shadow-sm"
        />
        <button
          type="submit"
          disabled={addPlayerMutation.isPending}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:bg-gray-400"
        >
          Add Player
        </button>
      </form>

      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}

      {isLoading ? (
        <p>Loading roster...</p>
      ) : (
        <ul className="space-y-2">
          {roster.map((player) => (
            <li
              key={player.id}
              className="flex justify-between items-center p-2 bg-sky-400 rounded"
            >
              <span>{player.name}</span>

              {/* --- CONDITIONAL BUTTON RENDERING --- */}
              <div className="flex space-x-2">
                {isAdmin ? (
                  <>
                    <button
                      onClick={() => removePlayerMutation.mutate(player.id)}
                      className="text-sm text-yellow-600 hover:underline"
                    >
                      Remove from Roster
                    </button>
                    <button
                      onClick={() => deletePlayerMutation.mutate(player.id)}
                      className="text-sm text-red-600 hover:underline"
                    >
                      Delete Permanently
                    </button>
                  </>
                ) : (
                  <button
                    onClick={() => removePlayerMutation.mutate(player.id)}
                    className="px-2 py-1 text-xs font-medium text-red-700 bg-red-100 rounded hover:bg-red-200"
                  >
                    Remove
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
