import { Link } from 'react-router-dom';

export default function Navbar() {
  return (
    <nav className="bg-gray-800 text-white p-4">
      {/* This div is updated */}
      <div className="flex justify-between items-center px-4 sm:px-6 lg:px-8">
        <Link to="/" className="text-xl font-bold">
          League Manager
        </Link>
        <div className="space-x-4">
          <Link to="/" className="hover:text-gray-300">Home</Link>
          <Link to="/standings" className="hover:text-gray-300">Standings</Link>
        </div>
      </div>
    </nav>
  );
}