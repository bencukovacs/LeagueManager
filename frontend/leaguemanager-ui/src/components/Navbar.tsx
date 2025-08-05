import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type { MyTeamResponse } from '../types';

// This fetch function is specific to the Navbar's needs
const fetchMyTeamStatus = async (): Promise<MyTeamResponse | null> => {
  try {
    const { data } = await apiClient.get('/my-team');
    return data;
  } catch (error) {
    return null;
  }
};

export default function Navbar() {
  const { user, isAuthenticated, logout } = useAuth();

  // This query runs only if the user is authenticated.
  // It fetches the user's team to determine which links to show.
  const { data: myTeamData } = useQuery({
    queryKey: ['myTeamStatus'],
    queryFn: fetchMyTeamStatus,
    enabled: isAuthenticated, // Only run this query if the user is logged in
  });

  const isManager = myTeamData?.userRole === 'Leader' || myTeamData?.userRole === 'AssistantLeader';

  return (
    <nav className="bg-gray-800 text-white p-4">
      <div className="container mx-auto flex justify-between items-center">
        <Link to="/" className="text-xl font-bold">
          League Manager
        </Link>
        <div className="flex items-center space-x-4">
          <Link to="/" className="hover:text-gray-300">Home</Link>
          <Link to="/standings" className="hover:text-gray-300">Standings</Link>
          
          {isAuthenticated ? (
            <>
              {user?.roles.includes('Admin') && (
                <Link to="/admin" className="hover:text-gray-300">Admin</Link>
              )}
              {isManager ? (
                // If they manage a team, show the "My Team" link
                <Link to="/my-team" className="hover:text-gray-300">My Team</Link>
              ) : (
                // Otherwise, show the "Create Team" link (unless they're an admin)
                !user?.roles.includes('Admin') && <Link to="/create-team" className="hover:text-gray-300">Create Team</Link>
              )}
              
              <button onClick={logout} className="hover:text-gray-300">
                Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/register" className="hover:text-gray-300">Register</Link>
              <Link to="/login" className="hover:text-gray-300">Login</Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}