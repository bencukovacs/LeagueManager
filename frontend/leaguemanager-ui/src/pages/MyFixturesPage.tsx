import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import apiClient from '../api/apiClient';
import type { FixtureResponseDto } from '../types';

// Function to fetch the current user's team fixtures (unchanged)
const fetchMyTeamFixtures = async (): Promise<FixtureResponseDto[]> => {
  const { data } = await apiClient.get('/my-team/fixtures');
  return data;
};

export default function MyFixturesPage() {
  const { data: fixtures, isLoading, isError } = useQuery({
    queryKey: ['myTeamFixtures'],
    queryFn: fetchMyTeamFixtures,
  });

  if (isLoading) return <div className="p-4">Loading fixtures...</div>;
  if (isError) return <div className="p-4 text-red-500">Failed to load fixtures.</div>;

  // We now filter based on the kick-off date compared to the current time.
  const now = new Date();
  const upcomingFixtures = fixtures?.filter(f => new Date(f.kickOffDateTime) > now) || [];
  const pastFixtures = fixtures?.filter(f => new Date(f.kickOffDateTime) <= now) || [];

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">My Fixtures</h1>

      <div className="mb-8">
        <h2 className="text-2xl font-semibold border-b pb-2 mb-4">Upcoming Matches</h2>
        {upcomingFixtures.length > 0 ? (
          <ul className="space-y-3">
            {upcomingFixtures.map(fixture => (
              <li key={fixture.id} className="p-3 border rounded-lg bg-sky-400 shadow-sm">
                <p>{new Date(fixture.kickOffDateTime).toLocaleString()}</p>
                <p className="text-lg font-medium">{fixture.homeTeam.name} vs {fixture.awayTeam.name}</p>
              </li>
            ))}
          </ul>
        ) : (
          <p>No upcoming matches scheduled.</p>
        )}
      </div>

      <div>
        <h2 className="text-2xl font-semibold border-b pb-2 mb-4">Past Matches</h2>
        {pastFixtures.length > 0 ? (
          <ul className="space-y-3">
            {pastFixtures.map(fixture => (
              <li key={fixture.id} className="p-3 border rounded-lg bg-sky-400 shadow-sm flex justify-between items-center">
                <div>
                  <p>{new Date(fixture.kickOffDateTime).toLocaleString()}</p>
                  <p className="text-lg font-medium">{fixture.homeTeam.name} vs {fixture.awayTeam.name}</p>
                </div>
                {/* Only show the "Submit Result" button if a result doesn't already exist */}
                {!fixture.result && (
                  <Link to={`/submit-result/${fixture.id}`} className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-300">
                    Submit Result
                  </Link>
                )}
                {fixture.result && (
                  <span className="px-4 py-2 text-sm text-gray-500">Result Submitted ({fixture.result.status})</span>
                )}
              </li>
            ))}
          </ul>
        ) : (
          <p>No past matches found.</p>
        )}
      </div>
    </div>
  );
}