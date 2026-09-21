import apiClient from '../../../shared/api/apiClient';
import { setToken, removeToken } from '../../../utils/token';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  userId: string;
  roles: string[];
  expiresAt: string;
}

export const login = async (credentials: LoginRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/login', credentials);
  
  if (response.data.token) {
    setToken(response.data.token);
  }
  
  return response.data;
};

export const logout = (): void => {
  removeToken();
  window.location.href = '/login';
};