import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { Team } from '../../types';

// This function sends the update request to the API
const updateTeam = async ({ teamId, teamData }: { teamId: number; teamData: { name: string; primaryColor: string; secondaryColor: string; } }) => {
  await apiClient.put(`/teams/${teamId}`, teamData);
};

interface EditTeamFormProps {
  team: Team;
}

export default function EditTeamForm({ team }: EditTeamFormProps) {
  const queryClient = useQueryClient();
  const [name, setName] = useState(team.name);
  const [primaryColor, setPrimaryColor] = useState(team.primaryColor || '');
  const [secondaryColor, setSecondaryColor] = useState(team.secondaryColor || '');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // This effect ensures the form fields update if the team data is refetched
  useEffect(() => {
    setName(team.name);
    setPrimaryColor(team.primaryColor || '');
    setSecondaryColor(team.secondaryColor || '');
  }, [team]);

  const updateTeamMutation = useMutation({
    mutationFn: updateTeam,
    onSuccess: () => {
      setSuccess('Team updated successfully!');
      // When the update is successful, refetch the 'myTeam' query to get the latest data
      queryClient.invalidateQueries({ queryKey: ['myTeam'] });
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to update team.');
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    updateTeamMutation.mutate({ 
      teamId: team.id, 
      teamData: { name, primaryColor, secondaryColor } 
    });
  };

  return (
    <div className="mt-6 p-4 border rounded-lg">
      <h3 className="text-xl font-semibold mb-4">Edit Team Details</h3>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Team Name</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700">Primary Color</label>
          <input
            type="text"
            value={primaryColor}
            onChange={(e) => setPrimaryColor(e.target.value)}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700">Secondary Color</label>
          <input
            type="text"
            value={secondaryColor}
            onChange={(e) => setSecondaryColor(e.target.value)}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
          />
        </div>
        <button
          type="submit"
          disabled={updateTeamMutation.isPending}
          className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400"
        >
          Save Changes
        </button>
      </form>
      {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
      {success && <p className="text-green-500 text-sm mt-2">{success}</p>}
    </div>
  );
}