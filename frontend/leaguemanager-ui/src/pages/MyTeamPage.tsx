import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import { Team } from '../types';
import { Link } from 'react-router-dom';

// Function to fetch the current user's managed team
const fetchMyTeam = async (): Promise<Team> => {
  const { data } = await apiClient.get('/my-team');
  return data;
};

export default function MyTeamPage() {
  const { data: myTeam, isLoading, isError, error } = useQuery({
    queryKey: ['myTeam'],
    queryFn: fetchMyTeam,
    // This will prevent the query from running again immediately after a failure
    retry: false, 
  });

  if (isLoading) {
    return <div className="p-4">Loading your team...</div>;
  }

  // This handles the case where the user is logged in but is not a team leader
  if (isError) {
    return (
      <div className="container mx-auto p-4 text-center">
        <h2 className="text-xl font-semibold text-red-600">Could not load team</h2>
        <p className="text-gray-600 mt-2">
          You do not currently manage a team. If you believe this is an error, please contact the League Admin.
        </p>
        <Link to="/create-team" className="mt-4 inline-block px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
          Create a Team
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold">{myTeam?.name}</h1>
      <p className={`mt-2 text-sm font-semibold ${myTeam?.status === 'Approved' ? 'text-green-600' : 'text-yellow-600'}`}>
        Status: {myTeam?.status}
      </p>
      <div className="mt-6">
        <h2 className="text-2xl font-semibold border-b pb-2 mb-4">Team Management</h2>
        {/* We will add the roster management and edit team components here */}
      </div>
    </div>
  );
}