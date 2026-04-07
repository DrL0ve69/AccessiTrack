export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  role: 'Admin' | 'Member';
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export interface User {
  userId: string;
  email: string;
  role: 'Admin' | 'Member';
}
