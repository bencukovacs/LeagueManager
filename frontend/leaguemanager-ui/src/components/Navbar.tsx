import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function Navbar() {
  const { isAuthenticated, logout } = useAuth(); // Get the auth state and logout function

  return (
    <nav className="bg-gray-800 text-white p-4">
      <div className="container mx-auto flex justify-between items-center">
        <Link to="/" className="text-xl font-bold">
          League Manager
        </Link>
        <div className="space-x-4">
          <Link to="/" className="hover:text-gray-300">Home</Link>
          <Link to="/standings" className="hover:text-gray-300">Standings</Link>
          
          {/* This is the conditional rendering logic */}
          {isAuthenticated ? (
            // If the user IS authenticated, show a Logout button
            <button onClick={logout} className="hover:text-gray-300">
              Logout
            </button>
          ) : (
            // If the user is NOT authenticated, show the Register and Login links
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