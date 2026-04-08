import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.model';

/**
 * Authentication Service for AccessiTrack.
 * Manages user authentication state, token storage, and session lifecycle.
 * 
 * Clean Architecture: This is part of the Core/Application layer.
 * It handles cross-cutting auth concerns and acts as a facade
 * for auth API operations.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;
  private readonly tokenKey = 'accessitrack_token';
  private readonly userKey = 'accessitrack_user';

  // Signals for reactive state management (Angular 16+)
  private readonly _user = signal<User | null>(this.loadUser());
  private readonly _token = signal<string | null>(this.loadToken());

  // Computed properties for derived state
  readonly user = computed(() => this._user());
  readonly token = computed(() => this._token());
  readonly isAuthenticated = computed(() => !!this._token());
  readonly isAdmin = computed(() => this._user()?.role === 'Admin');
  readonly isMember = computed(() => this._user()?.role === 'Member');

  /**
   * Login user with email and password.
   * @param credentials User login credentials
   * @returns Observable of AuthResponse
   */
  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => this.setSession(response)),
      catchError((error) => {
        console.error('Login failed:', error);
        return throwError(
          () => new Error(error.message || 'Email ou mot de passe incorrect.')
        );
      })
    );
  }

  /**
   * Register new user account.
   * @param data User registration data
   * @returns Observable of AuthResponse
   */
  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap((response) => this.setSession(response)),
      catchError((error) => {
        console.error('Registration failed:', error);
        return throwError(
          () => new Error(error.message || 'Erreur lors de l\'inscription.')
        );
      })
    );
  }

  /**
   * Logout user and clear session.
   * Redirects to login page.
   */
  logout(): void {
    this.clearSession();
    this.router.navigate(['/login']);
  }

  /**
   * Get current authentication token.
   * Used by auth interceptor.
   */
  getToken(): string | null {
    return this._token();
  }

  /**
   * Store session data (token and user) in memory and localStorage.
   * @param response AuthResponse from server
   */
  private setSession(response: AuthResponse): void {
    const user: User = {
      userId: response.userId,
      email: response.email,
      role: response.role,
    };

    // Store in localStorage for persistence across browser sessions
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.tokenKey, response.token);
      localStorage.setItem(this.userKey, JSON.stringify(user));
    }

    // Update signals (triggers reactivity)
    this._token.set(response.token);
    this._user.set(user);
  }

  /**
   * Clear session data from memory and localStorage.
   */
  private clearSession(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }

    this._token.set(null);
    this._user.set(null);
  }

  /**
   * Load token from localStorage on service initialization.
   * Handles SSR (checks if window is available).
   */
  private loadToken(): string | null {
    if (typeof window === 'undefined') return null;
    try {
      return localStorage.getItem(this.tokenKey);
    } catch {
      return null;
    }
  }

  /**
   * Load user from localStorage on service initialization.
   * Handles SSR (checks if window is available).
   */
  private loadUser(): User | null {
    if (typeof window === 'undefined') return null;
    try {
      const userJson = localStorage.getItem(this.userKey);
      return userJson ? JSON.parse(userJson) : null;
    } catch {
      return null;
    }
  }
}
