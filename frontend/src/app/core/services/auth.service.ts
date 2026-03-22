import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { BehaviorSubject, tap, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenKey = 'smartsure_auth_token';
  private userRoleKey = 'smartsure_auth_role';

  // BehaviorSubject to easily track auth state across app components
  private authStatusSource = new BehaviorSubject<boolean>(this.hasToken());
  public authStatus$ = this.authStatusSource.asObservable();

  constructor(private api: ApiService) {}

  login(credentials: { email: string; password: string }) {
    return this.api.post<{ token: string, role: string }>('auth/login', credentials).pipe(
      tap(res => {
        if (res.token) {
          localStorage.setItem(this.tokenKey, res.token);
          localStorage.setItem(this.userRoleKey, res.role);
          this.authStatusSource.next(true);
        }
      })
    );
  }

  register(userData: any) {
    return this.api.post<{ message: string }>('auth/register', userData);
  }

  verifyOtp(data: { email: string, otp: string }) {
    return this.api.post<{ message: string }>('auth/verify-register-otp', data);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userRoleKey);
    this.authStatusSource.next(false);
  }

  getProfile() {
    return this.api.get<any>('auth/me');
  }

  updateProfile(data: any) {
    return this.api.put<any>('auth/me', data);
  }

  changePassword(data: any) {
    return this.api.put<any>('auth/change-password', data);
  }

  refreshToken(data: { refreshToken: string }) {
    return this.api.post<any>('auth/refresh', data);
  }

  // Google Auth
  getGoogleLoginUrl() {
    return `${environment.apiUrl}/auth/google`; // Using Gateway URL
  }

  processGoogleCallback(code: string) {
    return this.api.get<any>(`auth/google/callback?code=${code}`);
  }

  // Users Management (Admin) via Identity Service
  getAllUsers(): Observable<any[]> {
    return this.api.get<any[]>('auth/users');
  }

  assignRole(userId: string, roleId: string): Observable<any> {
    return this.api.put<any>(`auth/users/${userId}/roles`, { roleId });
  }

  deleteUser(userId: string): Observable<any> {
    return this.api.delete<any>(`auth/users/${userId}`);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRole(): string | null {
    return localStorage.getItem(this.userRoleKey);
  }

  hasToken(): boolean {
    return !!this.getToken();
  }
}
