import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function Navbar() {
  const { user, isAuthenticated, logout } = useAuth(); // Get the full user object

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
              {/* This link only shows if the user is an Admin */}
              {user?.roles.includes('Admin') && (
                <Link to="/admin" className="hover:text-gray-300">Admin</Link>
              )}
              <Link to="/create-team" className="hover:text-gray-300">Create Team</Link>
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