import React, { useState, useEffect, type ReactNode } from 'react';
import { isAxiosError } from 'axios';
import { login as apiLogin } from '../services/authService';
import { getToken, setToken, removeToken } from '../utils/token';
import { type User, type LoginRequest, type AuthResponse } from '../types/auth';
import { AuthContext } from './AuthContext';

interface AuthProviderProps {
  children: ReactNode;
}

// Helper to extract user data from stored JWT on app refresh
const parseJwtUser = (storedToken: string): User | null => {
  try {
    const payloadBase64 = storedToken.split('.')[1];
    const decoded = JSON.parse(atob(payloadBase64));

    // Check expiration
    if (decoded.exp && decoded.exp * 1000 < Date.now()) {
      return null;
    }

    // Extract values matching your backend's claim structure
    const username =
      decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
      decoded.unique_name ||
      decoded.name ||
      '';

    const userId =
      decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      decoded.nameid ||
      decoded.sub ||
      '';

    const rawRoles =
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      decoded.role ||
      [];

    const roles = Array.isArray(rawRoles) ? rawRoles : [rawRoles].filter(Boolean);

    return { userId, username, roles };
  } catch (e) {
    console.error('Failed to parse JWT payload:', e);
    return null;
  }
};

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setTokenState] = useState<string | null>(getToken());
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Restore session from localStorage on initial load or refresh
  useEffect(() => {
    const initializeAuth = () => {
      const storedToken = getToken();
      if (storedToken) {
        const parsedUser = parseJwtUser(storedToken);
        if (parsedUser) {
          setUser(parsedUser);
        } else {
          removeToken();
          setTokenState(null);
          setUser(null);
        }
      }
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  const login = async (credentials: LoginRequest): Promise<void> => {
    try {
      const response: AuthResponse = await apiLogin(credentials);

      // Save token to localStorage and react state
      setToken(response.token);
      setTokenState(response.token);

      // Populate user directly from response payload
      setUser({
        userId: response.userId,
        username: response.username,
        roles: Array.isArray(response.roles) ? response.roles : [response.roles].filter(Boolean),
      });
    } catch (err: unknown) {
      if (isAxiosError(err)) {
        throw new Error(err.response?.data?.message || 'Login failed', { cause: err });
      }
      throw err;
    }
  };

  const logout = (): void => {
    removeToken();
    setTokenState(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!user && !!token,
        isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};