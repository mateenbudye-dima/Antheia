export interface User {
  userId: string;
  username: string;
  roles: string[];
  privileges: string[];
  orgId: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  userId: string;
  roles: string[];
  privileges: string[];
  org_id: number;
  expiresAt: string;
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}