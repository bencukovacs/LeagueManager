import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface ProtectedRouteProps {
  allowedRoles?: string[];
}

export default function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
  const { user, isAuthenticated } = useAuth();

  // 1. First, check if the user is logged in at all.
  if (!isAuthenticated || !user) {
    // If not, redirect them to the login page.
    return <Navigate to="/login" replace />;
  }

  // 2. If the route requires specific roles, check if the user has one of them.
  if (allowedRoles && !allowedRoles.some(role => user.roles.includes(role))) {
    // If they don't have the required role, redirect them to the home page.
    return <Navigate to="/" replace />;
  }

  // 3. If all checks pass, render the child route.
  return <Outlet />;
}