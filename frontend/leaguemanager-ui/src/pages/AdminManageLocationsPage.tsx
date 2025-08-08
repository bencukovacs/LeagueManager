import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { Location } from '../types';
import { useState } from 'react';
import EditLocationModal from '../features/admin/EditLocationModal';

// Function to fetch all locations
const fetchLocations = async (): Promise<Location[]> => {
  const { data } = await apiClient.get('/locations');
  return data;
};

// Function to create a new location
const createLocation = async (locationData: { name: string; address?: string; pitchNumber?: string }) => {
  await apiClient.post('/locations', locationData);
};

// Function to delete a location
const deleteLocation = async (locationId: number) => {
  await apiClient.delete(`/locations/${locationId}`);
};

export default function AdminManageLocationsPage() {
  const queryClient = useQueryClient();
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [pitchNumber, setPitchNumber] = useState('');
  
  // State for managing the edit modal
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [selectedLocation, setSelectedLocation] = useState<Location | null>(null);

  const { data: locations, isLoading } = useQuery({
    queryKey: ['allLocations'],
    queryFn: fetchLocations,
  });

  const createMutation = useMutation({
    mutationFn: createLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allLocations'] });
      setName('');
      setAddress('');
      setPitchNumber('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allLocations'] });
    },
    // You can add onError handling here if you want to show a notification
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate({ name, address, pitchNumber });
  };

  const handleEdit = (location: Location) => {
    setSelectedLocation(location);
    setIsEditModalOpen(true);
  };

  const handleDelete = (locationId: number) => {
    // A simple browser confirmation before deleting
    if (window.confirm('Are you sure you want to delete this location? This action cannot be undone.')) {
      deleteMutation.mutate(locationId);
    }
  };

  return (
    <>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {/* Left side: List of existing locations */}
        <div className="md:col-span-2">
          <h2 className="text-2xl font-semibold mb-4">Existing Locations</h2>
          {isLoading ? (
            <p>Loading locations...</p>
          ) : (
            <div className="space-y-2">
              {locations?.map((loc) => (
                <div key={loc.id} className="p-3 bg-sky-400 border rounded-lg shadow-sm">
                  <div className="flex justify-between items-start">
                    <div>
                      <p className="font-bold">{loc.name}</p>
                      <p className="text-sm text-gray-600">{loc.address || 'No address provided'}</p>
                      <p className="text-sm text-gray-500">Pitch: {loc.pitchNumber || 'N/A'}</p>
                    </div>
                    <div className="flex space-x-2 flex-shrink-0">
                      <button onClick={() => handleEdit(loc)} className="text-sm text-blue-600 hover:underline">Edit</button>
                      <button onClick={() => handleDelete(loc.id)} className="text-sm text-red-600 hover:underline">Remove</button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Right side: Form to add a new location */}
        <div>
          <h2 className="text-2xl font-semibold mb-4">Add New Location</h2>
          <form onSubmit={handleSubmit} className="p-4 bg-sky-400 border rounded-lg shadow-sm space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Location Name</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="mt-1 block w-full border-gray-300 rounded-md shadow-sm"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Address</label>
              <input
                type="text"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                className="mt-1 block w-full border-gray-300 rounded-md shadow-sm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Pitch Number</label>
              <input
                type="text"
                value={pitchNumber}
                onChange={(e) => setPitchNumber(e.target.value)}
                className="mt-1 block w-full border-gray-300 rounded-md shadow-sm"
              />
            </div>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="w-full px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400"
            >
              Add Location
            </button>
            {createMutation.isError && <p className="text-red-500 text-sm mt-2">Failed to add location.</p>}
            {deleteMutation.isError && <p className="text-red-500 text-sm mt-2">Failed to delete location. It may be in use.</p>}
          </form>
        </div>
      </div>

      <EditLocationModal 
        isOpen={isEditModalOpen} 
        onClose={() => setIsEditModalOpen(false)} 
        location={selectedLocation} 
      />
    </>
  );
}