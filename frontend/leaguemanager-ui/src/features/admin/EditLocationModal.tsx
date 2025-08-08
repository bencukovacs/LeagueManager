import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { Location } from '../../types';

interface EditLocationModalProps {
  location: Location | null;
  isOpen: boolean;
  onClose: () => void;
}

const updateLocation = async (locationData: Location) => {
  await apiClient.put(`/locations/${locationData.id}`, locationData);
};

export default function EditLocationModal({ location, isOpen, onClose }: EditLocationModalProps) {
  const queryClient = useQueryClient();
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [pitchNumber, setPitchNumber] = useState('');

  useEffect(() => {
    if (location) {
      setName(location.name);
      setAddress(location.address || '');
      setPitchNumber(location.pitchNumber || '');
    }
  }, [location]);

  const mutation = useMutation({
    mutationFn: updateLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allLocations'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!location) return;
    mutation.mutate({ id: location.id, name, address, pitchNumber });
  };

  if (!isOpen || !location) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-sky-400 p-6 rounded-lg shadow-xl w-full max-w-md">
        <h2 className="text-2xl font-bold mb-4">Edit Location</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label>Location Name</label>
            <input type="text" value={name} onChange={(e) => setName(e.target.value)} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required />
          </div>
          <div>
            <label>Address</label>
            <input type="text" value={address} onChange={(e) => setAddress(e.target.value)} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
          </div>
          <div>
            <label>Pitch Number</label>
            <input type="text" value={pitchNumber} onChange={(e) => setPitchNumber(e.target.value)} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
          </div>
          {mutation.isError && <p className="text-red-500 text-sm">Failed to update location.</p>}
          <div className="flex justify-end space-x-2 pt-4">
            <button type="button" onClick={onClose} className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">Cancel</button>
            <button type="submit" disabled={mutation.isPending} className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400">Save Changes</button>
          </div>
        </form>
      </div>
    </div>
  );
}