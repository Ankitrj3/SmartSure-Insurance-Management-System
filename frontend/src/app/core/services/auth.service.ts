import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenKey        = 'smartsure_auth_token';
  private readonly refreshTokenKey = 'smartsure_refresh_token'; // stored in browser localStorage
  private readonly userRoleKey     = 'smartsure_auth_role';

  // BehaviorSubject to easily track auth state across app components
  private authStatusSource = new BehaviorSubject<boolean>(this.hasToken());
  public  authStatus$      = this.authStatusSource.asObservable();

  constructor(private api: ApiService) {}

  // ─── Authentication ────────────────────────────────────────────────────────

  login(credentials: { email: string; password: string }) {
    return this.api.post<{ token: string; refreshToken: string; role: string }>(
      'auth/login',
      credentials
    ).pipe(
      tap(res => {
        if (res.token) {
          this.storeTokens(res.token, res.refreshToken, res.role);
          console.log('Login successful – tokens stored in browser cache:', { role: res.role });
        }
      })
    );
  }

  register(userData: any) {
    return this.api.post<{ message: string }>('auth/register', userData);
  }

  verifyOtp(data: { email: string; otp: string }) {
    return this.api.post<{ message: string }>('auth/verify-register-otp', data);
  }

  forgotPassword(data: { email: string }) {
    return this.api.post<{ message: string }>('auth/forgot-password', data);
  }

  resetPassword(data: any) {
    return this.api.post<{ message: string }>('auth/reset-password', data);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userRoleKey);
    this.authStatusSource.next(false);
  }

  // ─── Token Management (browser localStorage) ───────────────────────────────

  /** Persist access token, refresh token, and role to localStorage. */
  storeTokens(token: string, refreshToken: string, role: string): void {
    localStorage.setItem(this.tokenKey,        token);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
    localStorage.setItem(this.userRoleKey,     role || 'User');
    this.authStatusSource.next(true);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getRole(): string | null {
    return localStorage.getItem(this.userRoleKey);
  }

  hasToken(): boolean {
    return !!this.getToken();
  }

  /**
   * Call the backend /auth/refresh endpoint using the stored refresh token.
   * On success, rotates both tokens in localStorage automatically.
   */
  refreshAccessToken(): Observable<{ token: string; refreshToken: string; role: string }> {
    const storedRefreshToken = this.getRefreshToken();
    return this.api.post<{ token: string; refreshToken: string; role: string }>(
      'auth/refresh',
      { refreshToken: storedRefreshToken }
    ).pipe(
      tap(res => {
        if (res.token) {
          this.storeTokens(res.token, res.refreshToken, res.role);
          console.log('Access token refreshed – new tokens stored in browser cache.');
        }
      })
    );
  }

  // ─── Profile ───────────────────────────────────────────────────────────────

  getProfile() {
    return this.api.get<any>('auth/me');
  }

  updateProfile(data: any) {
    return this.api.put<any>('auth/me', data);
  }

  changePassword(data: any) {
    return this.api.put<any>('auth/change-password', data);
  }

  // ─── Google Auth ───────────────────────────────────────────────────────────

  getGoogleLoginUrl() {
    return `${environment.apiUrl}/auth/google`;
  }

  processGoogleCallback(code: string) {
    return this.api.get<any>(`auth/google/callback?code=${code}`);
  }

  // ─── Admin – User Management ───────────────────────────────────────────────

  getAllUsers(): Observable<any[]> {
    return this.api.get<any[]>('auth/users');
  }

  assignRole(userId: string, roleId: string): Observable<any> {
    return this.api.put<any>(`auth/users/${userId}/roles`, { roleId });
  }

  deleteUser(userId: string): Observable<any> {
    return this.api.delete<any>(`auth/users/${userId}`);
  }

  // ─── Auth State Helpers ────────────────────────────────────────────────────

  isAuthenticated(): boolean {
    return this.hasToken();
  }

  isAdmin(): boolean {
    const role = this.getRole();
    return role === 'Admin' || role === 'admin';
  }

  isUser(): boolean {
    const role = this.getRole();
    return role === 'User' || role === 'user';
  }
}
