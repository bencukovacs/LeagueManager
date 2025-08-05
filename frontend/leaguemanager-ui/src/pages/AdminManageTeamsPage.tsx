import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import apiClient from '../api/apiClient';
import type { Team } from '../types';
import { useState } from 'react';
import CreateTeamModal from '../features/admin/CreateTeamModal';

// Function to fetch all teams for the admin
const fetchAllTeams = async (): Promise<Team[]> => {
  const { data } = await apiClient.get('/teams/all');
  return data;
};

export default function AdminManageTeamsPage() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const { data: teams, isLoading, isError } = useQuery({
    queryKey: ['allTeamsForAdmin'],
    queryFn: fetchAllTeams,
  });

  if (isLoading) return <div className="p-4">Loading all teams...</div>;
  if (isError) return <div className="p-4 text-red-500">Failed to load teams.</div>;

  return (
    <>
      <div className="container mx-auto p-4">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold">Manage All Teams</h1>
          <button
            onClick={() => setIsModalOpen(true)}
            className="px-4 py-2 bg-white text-white rounded-md hover:bg-indigo-700"
          >
            + Create New Team
          </button>
        </div>

        <table className="min-w-full bg-gray-400 border">
          <thead className="bg-gray-600">
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

      <CreateTeamModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </>
  );
}