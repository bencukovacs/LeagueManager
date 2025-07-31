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
      { path: "create-team", element: <CreateTeamPage /> },
      { path: "teams/:teamId", element: <TeamDetailsPage /> },
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