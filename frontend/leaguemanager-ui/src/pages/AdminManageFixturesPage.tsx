import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { Team, Location, FixtureResponseDto } from '../types';
import { useState } from 'react';
import EditFixtureModal from '../features/admin/EditFixtureModal';

// Fetch functions
const fetchApprovedTeams = async (): Promise<Team[]> => {
  const { data } = await apiClient.get('/teams');
  return data;
};

const fetchLocations = async (): Promise<Location[]> => {
  const { data } = await apiClient.get('/locations');
  return data;
};

const fetchFixtures = async (): Promise<FixtureResponseDto[]> => {
  const { data } = await apiClient.get('/fixtures');
  return data;
};

// Create and Delete functions
const createFixture = async (fixtureData: { homeTeamId: number; awayTeamId: number; kickOffDateTime: string; locationId?: number }) => {
  await apiClient.post('/fixtures', fixtureData);
};

const deleteFixture = async (fixtureId: number) => {
  await apiClient.delete(`/fixtures/${fixtureId}`);
};

export default function AdminManageFixturesPage() {
  const queryClient = useQueryClient();
  
  // Form state for creating a new fixture
  const [homeTeamId, setHomeTeamId] = useState<number | undefined>();
  const [awayTeamId, setAwayTeamId] = useState<number | undefined>();
  const [locationId, setLocationId] = useState<number | undefined>();
  const [kickOff, setKickOff] = useState('');
  const [error, setError] = useState<string | null>(null);

  // State for managing the edit modal
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [selectedFixture, setSelectedFixture] = useState<FixtureResponseDto | null>(null);

  // Data fetching queries
  const { data: teams, isLoading: teamsLoading } = useQuery({ queryKey: ['approvedTeams'], queryFn: fetchApprovedTeams });
  const { data: locations, isLoading: locationsLoading } = useQuery({ queryKey: ['allLocations'], queryFn: fetchLocations });
  const { data: fixtures, isLoading: fixturesLoading } = useQuery({ queryKey: ['allFixtures'], queryFn: fetchFixtures });

  const createMutation = useMutation({
    mutationFn: createFixture,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allFixtures'] });
      // Reset form
      setHomeTeamId(undefined);
      setAwayTeamId(undefined);
      setLocationId(undefined);
      setKickOff('');
      setError(null);
    },
    onError: (err: any) => {
        setError(err.response?.data || "Failed to create fixture.");
    }
  });

  const deleteMutation = useMutation({
    mutationFn: deleteFixture,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allFixtures'] });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!homeTeamId || !awayTeamId || !kickOff) {
        setError("Home team, away team, and kickoff time are required.");
        return;
    }
    createMutation.mutate({ homeTeamId, awayTeamId, kickOffDateTime: kickOff, locationId });
  };

  const handleEdit = (fixture: FixtureResponseDto) => {
    setSelectedFixture(fixture);
    setIsEditModalOpen(true);
  };

  const handleDelete = (fixtureId: number) => {
    if (window.confirm('Are you sure you want to delete this fixture? This cannot be undone.')) {
      deleteMutation.mutate(fixtureId);
    }
  };

  return (
    <>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {/* Left side: List of existing fixtures */}
        <div className="md:col-span-2">
          <h2 className="text-2xl font-semibold mb-4">Scheduled Fixtures</h2>
          {fixturesLoading ? (
            <p>Loading fixtures...</p>
          ) : (
            <div className="space-y-3">
              {fixtures?.map((fixture) => (
                <div key={fixture.id} className="p-3 bg-sky-400 border rounded-lg shadow-sm">
                  <div className="flex justify-between items-start">
                    <div>
                      <p className="text-sm text-gray-500">{new Date(fixture.kickOffDateTime).toLocaleString()}</p>
                      <p className="text-lg font-medium">{fixture.homeTeam.name} vs {fixture.awayTeam.name}</p>
                      <p className="text-sm text-gray-600">Location: {fixture.location?.name || 'TBD'}</p>
                    </div>
                    <div className="flex space-x-2 flex-shrink-0">
                      <button onClick={() => handleEdit(fixture)} className="text-sm text-blue-600 hover:underline">Edit</button>
                      <button onClick={() => handleDelete(fixture.id)} className="text-sm text-red-600 hover:underline">Remove</button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Right side: Form to add a new fixture */}
        <div>
          <h2 className="text-2xl font-semibold mb-4">Create New Fixture</h2>
          <form onSubmit={handleSubmit} className="p-4 bg-sky-400 border rounded-lg shadow-sm space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Home Team</label>
              <select value={homeTeamId || ''} onChange={(e) => setHomeTeamId(Number(e.target.value))} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required>
                <option value="">Select a team</option>
                {teamsLoading ? <option>Loading...</option> : teams?.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Away Team</label>
              <select value={awayTeamId || ''} onChange={(e) => setAwayTeamId(Number(e.target.value))} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required>
                  <option value="">Select a team</option>
                  {teamsLoading ? <option>Loading...</option> : teams?.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Kickoff Time</label>
              <input
                type="datetime-local"
                value={kickOff}
                onChange={(e) => setKickOff(e.target.value)}
                className="mt-1 block w-full border-gray-300 rounded-md shadow-sm"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Location</label>
              <select value={locationId || ''} onChange={(e) => setLocationId(Number(e.target.value))} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm">
                  <option value="">Select a location (optional)</option>
                  {locationsLoading ? <option>Loading...</option> : locations?.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
              </select>
            </div>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="w-full px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400"
            >
              Create Fixture
            </button>
            {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
          </form>
        </div>
      </div>

      <EditFixtureModal 
        isOpen={isEditModalOpen} 
        onClose={() => setIsEditModalOpen(false)} 
        fixture={selectedFixture} 
      />
    </>
  );
}