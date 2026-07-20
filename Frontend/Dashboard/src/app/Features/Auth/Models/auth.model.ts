export interface SignupRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  emailOrUsername: string;
  password: string;
}

export type UserRole = 'User' | 'Admin' | 'SuperAdmin';

export interface AuthResponse {
  userId: string;
  username: string;
  email: string;
  role: UserRole;
  // JWT access + refresh tokens. Present on signup/login/refresh; empty on profile responses.
  token?: string;
  refreshToken?: string;
  expiresAt?: string | null;
}

export interface UpdateProfileRequest {
  username?: string;
  email?: string;
  currentPassword?: string;
  newPassword?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface MessageResponse {
  message: string;
}

// Safe user projection returned by the admin user-list (no tokens/hashes).
export interface UserSummary {
  id: string;
  username: string;
  email: string;
  role: UserRole;
}
