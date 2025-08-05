import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import apiClient from '../api/apiClient';
import type { Team } from '../types';

// Function to fetch all teams for the admin
const fetchAllTeams = async (): Promise<Team[]> => {
  const { data } = await apiClient.get('/teams/all');
  return data;
};

export default function AdminManageTeamsPage() {
  const { data: teams, isLoading, isError } = useQuery({
    queryKey: ['allTeamsForAdmin'],
    queryFn: fetchAllTeams,
  });

  if (isLoading) return <div className="p-4">Loading all teams...</div>;
  if (isError) return <div className="p-4 text-red-500">Failed to load teams.</div>;

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Manage All Teams</h1>
      <table className="min-w-full bg-white border">
        <thead className="bg-gray-200">
          <tr>
            <th className="py-2 px-4 border-b">Team Name</th>
            <th className="py-2 px-4 border-b">Status</th>
            <th className="py-2 px-4 border-b">Primary Color</th>
          </tr>
        </thead>
        <tbody>
          {teams?.map((team) => (
            <tr key={team.id} className="text-center hover:bg-gray-50">
              <td className="py-2 px-4 border-b text-left">
                <Link to={`/teams/${team.id}`} className="text-blue-600 hover:underline">
                  {team.name}
                </Link>
              </td>
              <td className="py-2 px-4 border-b">{team.status}</td>
              <td className="py-2 px-4 border-b">{team.primaryColor || 'N/A'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}