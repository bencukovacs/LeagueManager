import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';

interface CreateTeamModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const createTeamAsAdmin = async (teamData: { name: string; primaryColor: string; secondaryColor: string; }) => {
  await apiClient.post('/teams/admin', teamData);
};

export default function CreateTeamModal({ isOpen, onClose }: CreateTeamModalProps) {
  const queryClient = useQueryClient();
  const [name, setName] = useState('');
  const [primaryColor, setPrimaryColor] = useState('');
  const [secondaryColor, setSecondaryColor] = useState('');
  const [error, setError] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: createTeamAsAdmin,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allTeamsForAdmin'] });
      onClose(); // Close the modal on success
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to create team.');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate({ name, primaryColor, secondaryColor });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-sky-400 p-6 rounded-lg shadow-xl w-full max-w-md">
        <h2 className="text-2xl font-bold mb-4">Create New Team (Admin)</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Form fields are the same as the user-facing CreateTeamPage */}
          <div>
            <label>Team Name</label>
            <input type="text" value={name} onChange={(e) => setName(e.target.value)} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required />
          </div>
          <div>
            <label>Primary Color</label>
            <input type="text" value={primaryColor} onChange={(e) => setPrimaryColor(e.target.value)} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
          </div>
          <div>
            <label>Secondary Color</label>
            <input type="text" value={secondaryColor} onChange={(e) => setSecondaryColor(e.target.value)} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
          </div>
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex justify-end space-x-2 pt-4">
            <button type="button" onClick={onClose} className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">Cancel</button>
            <button type="submit" disabled={mutation.isPending} className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-400">Create</button>
          </div>
        </form>
      </div>
    </div>
  );
}