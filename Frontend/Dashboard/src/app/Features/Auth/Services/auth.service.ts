import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ConfigService } from '../../../Core/Services/config.service';
import {
  AuthResponse,
  ForgotPasswordRequest,
  LoginRequest,
  MessageResponse,
  ResetPasswordRequest,
  SignupRequest,
  UpdateProfileRequest,
  UserRole,
  UserSummary
} from '../Models/auth.model';

const STORAGE_KEY = 'auth_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl: string;
  readonly currentUser = signal<AuthResponse | null>(AuthService.loadFromStorage());

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.apiUrl = this.configService.baseUrl + '/auth';
  }

  signup(payload: SignupRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/signup`, payload).pipe(
      tap((user) => this.persistUser(user))
    );
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, payload).pipe(
      tap((user) => this.persistUser(user))
    );
  }

  getProfile(userId: string): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(`${this.apiUrl}/users/${userId}`);
  }

  updateProfile(userId: string, payload: UpdateProfileRequest): Observable<AuthResponse> {
    return this.http.put<AuthResponse>(`${this.apiUrl}/users/${userId}`, payload).pipe(
      tap((user) => this.persistUser(user))
    );
  }

  // SuperAdmin only (enforced server-side); lists users for the role-management picker.
  getUsers(search?: string): Observable<UserSummary[]> {
    const query = search?.trim() ? `?search=${encodeURIComponent(search.trim())}` : '';
    return this.http.get<UserSummary[]>(`${this.apiUrl}/users${query}`);
  }

  // SuperAdmin only (enforced server-side); assigns User/Admin to a user by id.
  assignRole(userId: string, role: UserRole): Observable<AuthResponse> {
    return this.http.put<AuthResponse>(`${this.apiUrl}/users/${userId}/role`, { role });
  }

  // Exchanges the stored refresh token for a fresh access + refresh token.
  refresh(): Observable<AuthResponse> {
    const refreshToken = this.currentUser()?.refreshToken ?? '';
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap((user) => this.persistUser(user))
    );
  }

  getToken(): string | null {
    return this.currentUser()?.token ?? null;
  }

  isAdmin(): boolean {
    const role = this.currentUser()?.role;
    return role === 'Admin' || role === 'SuperAdmin';
  }

  isSuperAdmin(): boolean {
    return this.currentUser()?.role === 'SuperAdmin';
  }

  forgotPassword(payload: ForgotPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/forgot-password`, payload);
  }

  resetPassword(payload: ResetPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/reset-password`, payload);
  }

  logout(): void {
    // Best-effort server-side revocation of the refresh token; clear locally regardless.
    if (this.currentUser()?.token) {
      this.http.post(`${this.apiUrl}/logout`, {}).subscribe({ next: () => {}, error: () => {} });
    }
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(STORAGE_KEY);
    }
    this.currentUser.set(null);
  }

  // Merges the response into stored state. Responses without tokens (profile reads/updates)
  // preserve the existing token/refreshToken so the session isn't lost.
  private persistUser(user: AuthResponse): void {
    const existing = this.currentUser();
    const merged: AuthResponse = {
      ...user,
      token: user.token || existing?.token || '',
      refreshToken: user.refreshToken || existing?.refreshToken || '',
      expiresAt: user.expiresAt ?? existing?.expiresAt ?? null
    };
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(merged));
    }
    this.currentUser.set(merged);
  }

  private static loadFromStorage(): AuthResponse | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthResponse;
    } catch {
      return null;
    }
  }
}
