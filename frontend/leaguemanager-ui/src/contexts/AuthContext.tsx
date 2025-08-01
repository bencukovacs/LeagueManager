import { createContext, useState, useContext, useEffect, type ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';
import apiClient from '../api/apiClient';

// Define the shape of our user object
interface User {
  id: string;
  email: string;
  roles: string[];
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  // This effect runs once on initial load to check for an existing token
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const decodedToken: { sub: string; email: string; role: string | string[] } = jwtDecode(token);
        
        // Check if token is expired
        const tokenExp = (jwtDecode(token) as any).exp;
        if (Date.now() >= tokenExp * 1000) {
          logout();
        } else {
          const roles = Array.isArray(decodedToken.role) ? decodedToken.role : [decodedToken.role];
          setUser({ id: decodedToken.sub, email: decodedToken.email, roles });
          apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        }
      } catch (error) {
        console.error("Failed to decode token on initial load", error);
        logout();
      }
    }
  }, []);

  const login = (token: string) => {
    try {
      const decodedToken: { sub: string; email: string; role: string | string[] } = jwtDecode(token);
      const roles = Array.isArray(decodedToken.role) ? decodedToken.role : [decodedToken.role];
      setUser({ id: decodedToken.sub, email: decodedToken.email, roles });
      localStorage.setItem('authToken', token);
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } catch (error) {
      console.error("Failed to decode token on login", error);
      logout();
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('authToken');
    delete apiClient.defaults.headers.common['Authorization'];
  };

  const value = {
    user,
    isAuthenticated: !!user,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}