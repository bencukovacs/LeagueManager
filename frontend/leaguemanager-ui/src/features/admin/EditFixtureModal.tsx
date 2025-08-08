import { useState, useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { FixtureResponseDto, Location } from '../../types';

interface EditFixtureModalProps {
  fixture: FixtureResponseDto | null;
  isOpen: boolean;
  onClose: () => void;
}

// API Functions
const updateFixture = async (payload: { fixtureId: number; data: any }) => {
  await apiClient.put(`/fixtures/${payload.fixtureId}`, payload.data);
};

const fetchLocations = async (): Promise<Location[]> => {
    const { data } = await apiClient.get('/locations');
    return data;
};

export default function EditFixtureModal({ fixture, isOpen, onClose }: EditFixtureModalProps) {
  const queryClient = useQueryClient();
  const [kickOff, setKickOff] = useState('');
  const [locationId, setLocationId] = useState<number | undefined>();

  // Fetch locations for the dropdown
  const { data: locations } = useQuery({ queryKey: ['allLocations'], queryFn: fetchLocations });

  useEffect(() => {
    if (fixture) {
      // Format the date correctly for the datetime-local input
      const date = new Date(fixture.kickOffDateTime);
      const formattedDate = date.toISOString().slice(0, 16);
      setKickOff(formattedDate);
      setLocationId(fixture.location?.id);
    }
  }, [fixture]);

  const mutation = useMutation({
    mutationFn: updateFixture,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allFixtures'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!fixture) return;
    mutation.mutate({ fixtureId: fixture.id, data: { kickOffDateTime: kickOff, locationId } });
  };

  if (!isOpen || !fixture) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-sky-400 p-6 rounded-lg shadow-xl w-full max-w-md">
        <h2 className="text-2xl font-bold mb-4">Edit Fixture</h2>
        <p className="mb-4 text-gray-600">{fixture.homeTeam.name} vs {fixture.awayTeam.name}</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label>Kickoff Time</label>
            <input
              type="datetime-local"
              value={kickOff}
              onChange={(e) => setKickOff(e.target.value)}
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm"
              required
            />
          </div>
          <div>
            <label>Location</label>
            <select value={locationId || ''} onChange={(e) => setLocationId(Number(e.target.value))} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm">
                <option value="">Select a location (optional)</option>
                {locations?.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
            </select>
          </div>
          {mutation.isError && <p className="text-red-500 text-sm">Failed to update fixture.</p>}
          <div className="flex justify-end space-x-2 pt-4">
            <button type="button" onClick={onClose} className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">Cancel</button>
            <button type="submit" disabled={mutation.isPending} className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400">Save Changes</button>
          </div>
        </form>
      </div>
    </div>
  );
}