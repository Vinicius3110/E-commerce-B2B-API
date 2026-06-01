import React, { createContext, useState, useEffect, useContext } from 'react';
import { authService } from '../services/auth';
import type { UserProfile } from '../services/auth';

interface AuthContextType {
  isAuthenticated: boolean;
  user: UserProfile | null;
  mode: 'buyer' | 'seller';
  login: (email: string, password: string) => Promise<void>;
  register: (payload: any) => Promise<void>;
  logout: () => void;
  toggleMode: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(!!localStorage.getItem('access_token'));
  const [mode, setMode] = useState<'buyer' | 'seller'>('buyer');
  const [loading, setLoading] = useState<boolean>(true);

  const parseJwt = (token: string) => {
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  };

  const loadUserFromToken = () => {
    const token = localStorage.getItem('access_token');
    if (token) {
      const decoded = parseJwt(token);
      if (decoded) {
        setUser({
          id: decoded.sub,
          name: decoded.name || 'Administrador',
          email: decoded.email || '',
          companyId: decoded.company_id,
          companyName: decoded.company_name || 'Minha Empresa B2B',
        });
        setIsAuthenticated(true);
      }
    }
    setLoading(false);
  };

  useEffect(() => {
    loadUserFromToken();
    
    const handleExpired = () => logout();
    window.addEventListener('auth-expired', handleExpired);
    return () => window.removeEventListener('auth-expired', handleExpired);
  }, []);

  useEffect(() => {
    document.body.className = mode === 'buyer' ? 'theme-buyer' : 'theme-seller';
  }, [mode]);

  const login = async (email: string, password: string) => {
    const response = await authService.login(email, password);
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    setIsAuthenticated(true);
    loadUserFromToken();
  };

  const register = async (payload: any) => {
    const response = await authService.register(payload);
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    setIsAuthenticated(true);
    loadUserFromToken();
  };

  const logout = () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    setUser(null);
    setIsAuthenticated(false);
  };

  const toggleMode = () => {
    setMode((prev) => (prev === 'buyer' ? 'seller' : 'buyer'));
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, user, mode, login, register, logout, toggleMode, loading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  return context;
};
