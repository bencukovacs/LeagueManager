import React from 'react'
import ReactDOM from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from './contexts/AuthContext'
import './index.css'

import RootLayout from './pages/RootLayout' 
import HomePage from './pages/HomePage'
import StandingsPage from './pages/StandingsPage'
import RegisterPage from './pages/RegisterPage'
import LoginPage from './pages/LoginPage'
import CreateTeamPage from './pages/CreateTeamPage'
import TeamDetailsPage from './pages/TeamDetailsPage'
import AdminDashboardPage from './pages/AdminDashboardPage'
import ProtectedRoute from './components/ProtectedRoute'
import MyTeamPage from './pages/MyTeamPage'
import AdminManageTeamsPage from './pages/AdminManageTeamsPage'
import AdminLayout from './pages/AdminLayout'

const queryClient = new QueryClient()

const router = createBrowserRouter([
  {
    path: "/",
    element: <RootLayout />, 
    children: [             
      { index: true, element: <HomePage /> },
      { path: "standings", element: <StandingsPage /> },
      { path: "register", element: <RegisterPage /> },
      { path: "login", element: <LoginPage /> },
      { path: "teams/:teamId", element: <TeamDetailsPage /> },

      // --- ROUTES FOR ANY LOGGED-IN USER ---
      {
        element: <ProtectedRoute />, // No roles needed, just authentication
        children: [
          { path: "create-team", element: <CreateTeamPage /> },
          { path: "my-team", element: <MyTeamPage /> },
          // Add other general user routes here later
        ],
      },

      // --- ROUTES FOR ADMINS ONLY ---
      {
        path: "admin", // The base path for all admin pages
        element: <ProtectedRoute allowedRoles={['Admin']} />, // Protect the entire section
        children: [
          {
            element: <AdminLayout />, // Use the new layout as the shell
            children: [
              { index: true, element: <AdminDashboardPage /> }, // /admin shows the dashboard
              { path: "teams", element: <AdminManageTeamsPage /> }, // /admin/teams shows the management page
            ],
          },
        ],
      },
    ],
  },
]);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>
  </React.StrictMode>,
)