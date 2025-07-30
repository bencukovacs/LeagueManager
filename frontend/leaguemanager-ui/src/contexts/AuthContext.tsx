import { createContext, useState, useContext, useEffect, type ReactNode } from 'react';
import apiClient from '../api/apiClient';

// Define the shape of the context's value
interface AuthContextType {
  token: string | null;
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
}

// Create the context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Create the Provider component
export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => {
    // On initial load, check local storage for a token
    return localStorage.getItem('authToken');
  });

  // This effect runs whenever the token changes
  useEffect(() => {
    if (token) {
      // If there's a token, save it and set it on our API client
      localStorage.setItem('authToken', token);
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      // If the token is null (logged out), remove it
      localStorage.removeItem('authToken');
      delete apiClient.defaults.headers.common['Authorization'];
    }
  }, [token]);

  const login = (newToken: string) => {
    setToken(newToken);
  };

  const logout = () => {
    setToken(null);
  };

  const value = {
    token,
    isAuthenticated: !!token, // Convert token string to a boolean
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// 4. Create a custom hook for easy access to the context
export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}