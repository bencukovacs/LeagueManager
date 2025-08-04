import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { PlayerResponseDto } from '../../types';
import { useState } from 'react';

// The addPlayer function is unchanged
const addPlayer = async ({ teamId, name }: { teamId: number; name: string }) => {
  await apiClient.post('/players', { name, teamId });
};

// The component now receives the roster and loading state as props
interface RosterManagementProps {
  teamId: number;
  roster: PlayerResponseDto[];
  isLoading: boolean;
}

export default function RosterManagement({ teamId, roster, isLoading }: RosterManagementProps) {
  const queryClient = useQueryClient();
  const [newPlayerName, setNewPlayerName] = useState('');
  const [error, setError] = useState<string | null>(null);

  // The mutation logic is unchanged
  const addPlayerMutation = useMutation({
    mutationFn: addPlayer,
    onSuccess: () => {
      // This will now correctly trigger a refetch in the parent component
      queryClient.invalidateQueries({ queryKey: ['roster', teamId] });
      setNewPlayerName('');
      setError(null);
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to add player.');
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
          {/* The component now renders the roster passed in via props */}
          {roster.map((player) => (
            <li key={player.id} className="p-2 bg-gray-100 rounded">
              {player.name}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}