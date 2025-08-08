import { NavLink, Outlet } from 'react-router-dom';

export default function AdminLayout() {
  // Helper function to style the active tab
  const getNavLinkClass = ({ isActive }: { isActive: boolean }) =>
    `px-4 py-2 rounded-md text-sm font-medium ${
      isActive ? 'bg-indigo-600 text-white' : 'text-gray-700 hover:bg-gray-200'
    }`;

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Admin Area</h1>
      
      {/* Tab Navigation */}
      <div className="mb-6 border-b border-gray-200">
        <nav className="flex space-x-4">
          <NavLink to="/admin" end className={getNavLinkClass}>
            Dashboard
          </NavLink>
          <NavLink to="/admin/teams" className={getNavLinkClass}>
            Manage Teams
          </NavLink>
          <NavLink to="/admin/locations" className={getNavLinkClass}>
            Manage Locations
          </NavLink>
          <NavLink to="/admin/fixtures" className={getNavLinkClass}>
            Manage Fixtures
          </NavLink>
          <NavLink to="/admin/players" className={getNavLinkClass}>
            Manage Players
          </NavLink>
          {/* We can add more tabs here later */}
        </nav>
      </div>

      {/* Content for the selected tab will be rendered here */}
      <main>
        <Outlet />
      </main>
    </div>
  );
}