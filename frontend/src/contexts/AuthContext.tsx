/* eslint-disable react-refresh/only-export-components */
import { createContext, useState, useEffect, useContext, useCallback } from 'react';
import type { FC, ReactNode } from 'react';
import { authService } from '../services/auth';
import type { UserProfile, RegisterPayload } from '../services/auth';

interface AuthContextType {
  isAuthenticated: boolean;
  user: UserProfile | null;
  mode: 'buyer' | 'seller';
  login: (email: string, password: string) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => void;
  toggleMode: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Decodes a JWT token safely, supporting base64url normalization
 * and UTF-8 characters decoding (avoiding garbled text for Portuguese characters).
 */
const parseJwt = (token: string) => {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const pad = base64.length % 4;
    const paddedBase64 = pad ? base64 + '='.repeat(4 - pad) : base64;

    const jsonPayload = decodeURIComponent(
      atob(paddedBase64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );

    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
};

const getUserFromStorage = (): UserProfile | null => {
  const token = localStorage.getItem('access_token');
  if (!token) return null;
  const decoded = parseJwt(token);
  if (!decoded) return null;
  return {
    id: decoded.sub,
    name: decoded.name || 'Administrador',
    email: decoded.email || '',
    companyId: decoded.company_id,
    companyName: decoded.company_name || 'Minha Empresa B2B',
  };
};

export const AuthProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserProfile | null>(() => getUserFromStorage());
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(() => !!localStorage.getItem('access_token'));
  const [mode, setMode] = useState<'buyer' | 'seller'>('buyer');
  const [loading, setLoading] = useState<boolean>(false);

  const loadUserFromToken = useCallback(() => {
    const u = getUserFromStorage();
    if (u) {
      setUser(u);
      setIsAuthenticated(true);
    } else {
      setUser(null);
      setIsAuthenticated(false);
    }
    setLoading(false);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    setUser(null);
    setIsAuthenticated(false);
  }, []);

  useEffect(() => {
    const handleExpired = () => logout();
    window.addEventListener('auth-expired', handleExpired);
    return () => window.removeEventListener('auth-expired', handleExpired);
  }, [logout]);

  useEffect(() => {
    document.body.className = mode === 'buyer' ? 'theme-buyer' : 'theme-seller';
  }, [mode]);

  const login = useCallback(async (email: string, password: string) => {
    const response = await authService.login(email, password);
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    setIsAuthenticated(true);
    loadUserFromToken();
  }, [loadUserFromToken]);

  const register = useCallback(async (payload: RegisterPayload) => {
    const response = await authService.register(payload);
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    setIsAuthenticated(true);
    loadUserFromToken();
  }, [loadUserFromToken]);

  const toggleMode = useCallback(() => {
    setMode((prev) => (prev === 'buyer' ? 'seller' : 'buyer'));
  }, []);

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

