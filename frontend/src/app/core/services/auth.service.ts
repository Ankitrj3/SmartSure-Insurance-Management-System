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
          
          // Ensure role is stored properly
          const role = res.role || 'User';
          localStorage.setItem(this.userRoleKey, role);
          
          this.authStatusSource.next(true);
          
          console.log('Login successful - Token and role stored:', { role });
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

  forgotPassword(data: { email: string }) {
    return this.api.post<{ message: string }>('auth/forgot-password', data);
  }

  resetPassword(data: any) {
    return this.api.post<{ message: string }>('auth/reset-password', data);
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
    const token = localStorage.getItem(this.tokenKey);
    return token;
  }

  getRole(): string | null {
    const role = localStorage.getItem(this.userRoleKey);
    return role;
  }

  hasToken(): boolean {
    const token = this.getToken();
    return !!token;
  }

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
