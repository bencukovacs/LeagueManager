import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type {PlayerResponseDto} from '../../types';
import { useState } from 'react';
import { AxiosError } from 'axios';

// API Functions
const addPlayer = async ({ teamId, name }: { teamId: number; name: string }) => {
    await apiClient.post('/players', { name, teamId });
};
const removePlayerFromRoster = async (playerId: number) => {
    await apiClient.patch(`/players/${playerId}/remove-from-roster`);
};
const deletePlayerPermanently = async (playerId: number) => {
    await apiClient.delete(`/players/${playerId}`);
};
const updateMemberRole = async ({ teamId, userId, newRole }: { teamId: number; userId: string; newRole: number }) => {
    await apiClient.put(`/teams/${teamId}/members/${userId}/role`, { newRole });
};
// NEW: API function for demoting
const demoteMember = async ({ teamId, userId }: { teamId: number; userId: string; }) => {
    await apiClient.patch(`/teams/${teamId}/members/${userId}/demote`);
};

interface RosterManagementProps {
    teamId: number;
    roster: PlayerResponseDto[];
    isLoading: boolean;
    isAdmin: boolean;
    currentUserRole: 'Leader' | 'AssistantLeader' | 'Member';
}

export default function RosterManagement({ teamId, roster, isLoading, isAdmin, currentUserRole }: RosterManagementProps) {
    const queryClient = useQueryClient();
    const [newPlayerName, setNewPlayerName] = useState('');
    const [error, setError] = useState<string | null>(null);

    const isLeader = currentUserRole === 'Leader';

    // Mutations
    const addPlayerMutation = useMutation({
        mutationFn: addPlayer,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['roster'] });
            setNewPlayerName('');
            setError(null);
        },
        onError: (err: AxiosError<{ message: string }>) => {
            setError(err.response?.data?.message || 'Failed to add player.');
        }
    });
    const removePlayerMutation = useMutation({
        mutationFn: removePlayerFromRoster,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['roster'] });
        },
        onError: (err: AxiosError<{ message: string }>) => {
            setError(err.response?.data?.message || 'Failed to remove player.');
        }
    });
    const deletePlayerMutation = useMutation({
        mutationFn: deletePlayerPermanently,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['roster'] });
        },
        onError: (err: AxiosError<{ message: string }>) => {
            setError(err.response?.data?.message || 'Failed to permanently delete player.');
        }
    });
    const updateRoleMutation = useMutation({
        mutationFn: updateMemberRole,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['roster'] });
            queryClient.invalidateQueries({ queryKey: ['myTeam'] });
        },
        onError: (err: AxiosError<{ message: string }>) => {
            setError(err.response?.data?.message || 'Failed to update role.');
        }
    });
    // NEW: Mutation for demoting
    const demoteMutation = useMutation({
        mutationFn: demoteMember,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['roster'] });
        }
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!newPlayerName.trim()) return;
        addPlayerMutation.mutate({ teamId, name: newPlayerName });
    };
    const handleRemove = (playerId: number) => {
        if (window.confirm('Are you sure you want to remove this player from the roster?')) {
            removePlayerMutation.mutate(playerId);
        }
    };
    const handleDelete = (playerId: number) => {
        if (window.confirm('Are you sure you want to PERMANENTLY DELETE this player? This action cannot be undone.')) {
            deletePlayerMutation.mutate(playerId);
        }
    };
    // NEW: Specific handlers for role changes
    const handleMakeAssistant = (userId: string) => {
        updateRoleMutation.mutate({ teamId, userId, newRole: 1 }); // 1 = AssistantLeader
    };
    const handleHandoverLeadership = (userId: string) => {
        if (window.confirm('Are you sure you want to transfer leadership? You will become an Assistant Leader.')) {
            updateRoleMutation.mutate({ teamId, userId, newRole: 0 }); // 0 = Leader
        }
    };

    return (
        <div className="p-4 border rounded-lg bg-sky-400">
            <h3 className="text-xl font-semibold mb-4">Roster & Team Members</h3>

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
                        <li key={player.id} className="p-2 bg-sky-500 border rounded">
                            <div className="flex justify-between items-center">
                                <div className="flex flex-col">
                                    <span>{player.name}</span>
                                    {player.userRole && <span className="text-xs text-gray-200">{player.userRole}</span>}
                                </div>
                                <div className="flex items-center space-x-2">
                                    {/* --- THIS IS THE NEW BUTTON-BASED UI --- */}
                                    {isLeader && player.userRole === 'Member' && (
                                        <button onClick={() => handleMakeAssistant(player.userId!)} className="text-sm text-blue-200 hover:underline">Make Assistant</button>
                                    )}
                                    {isLeader && player.userRole === 'AssistantLeader' && (
                                        <>
                                            <button onClick={() => handleHandoverLeadership(player.userId!)} className="text-sm text-green-200 hover:underline">Make Leader</button>
                                            <button onClick={() => demoteMutation.mutate({ teamId, userId: player.userId! })} className="text-sm text-yellow-200 hover:underline">Demote</button>
                                        </>
                                    )}

                                    {isAdmin ? (
                                        <>
                                            <button onClick={() => handleRemove(player.id)} className="text-sm text-yellow-200 hover:underline">Remove</button>
                                            <button onClick={() => handleDelete(player.id)} className="text-sm text-red-300 hover:underline">Delete</button>
                                        </>
                                    ) : (
                                        <button onClick={() => handleRemove(player.id)} className="px-2 py-1 text-xs font-medium text-red-700 bg-red-100 rounded hover:bg-red-200">
                                            Remove
                                        </button>
                                    )}
                                </div>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}