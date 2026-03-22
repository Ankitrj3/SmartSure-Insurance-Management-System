import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private api: ApiService) {}

  // ── User Management ─────────────────────────────────────────────────────────
  // getAllUsers goes through Identity Service via gateway: GET /auth/users
  getAllUsers(): Observable<any[]> {
    return this.api.get<any[]>('auth/users');
  }

  // GET /admin/users/:userId  → AdminUsersController → proxies to Identity
  getUser(userId: string): Observable<any> {
    return this.api.get<any>(`admin/users/${userId}`);
  }

  // DELETE /admin/users/:userId → AdminUsersController → proxies to Identity
  deactivateUser(userId: string): Observable<any> {
    return this.api.delete<any>(`admin/users/${userId}`);
  }

  assignRole(userId: string, roleId: string): Observable<any> {
    return this.api.put<any>(`auth/users/${userId}/roles`, { roleId });
  }

  deleteUser(userId: string): Observable<any> {
    return this.api.delete<any>(`auth/users/${userId}`);
  }

  // ── Claims Management ────────────────────────────────────────────────────────
  // GET /claims/all → ClaimsController (Admin role required)
  getAllClaims(): Observable<any[]> {
    return this.api.get<any[]>('claims/all');
  }

  // GET /admin/claims/statistics → AdminClaimsController
  getClaimStatistics(): Observable<any> {
    return this.api.get<any>('admin/claims/statistics');
  }

  // PUT /admin/claims/:claimId/review → AdminClaimsController
  reviewClaim(claimId: string, notes: string = ''): Observable<any> {
    return this.api.put<any>(`admin/claims/${claimId}/review`, { notes });
  }

  // PUT /admin/claims/:claimId/approve → AdminClaimsController
  approveClaim(claimId: string, approvedAmount: number, notes: string = ''): Observable<any> {
    return this.api.put<any>(`admin/claims/${claimId}/approve`, { approvedAmount, notes });
  }

  // PUT /admin/claims/:claimId/reject → AdminClaimsController
  rejectClaim(claimId: string, reason: string): Observable<any> {
    return this.api.put<any>(`admin/claims/${claimId}/reject`, { reason });
  }

  // ── Policy Management ────────────────────────────────────────────────────────
  // GET /policies/all → PoliciesController (Admin role required)
  getAllPolicies(): Observable<any[]> {
    return this.api.get<any[]>('policies/all');
  }

  // ── Dashboard ────────────────────────────────────────────────────────────────
  // GET /admin/dashboard → DashboardController
  getDashboardStats(): Observable<any> {
    return this.api.get<any>('admin/dashboard');
  }

  // ── Reports ──────────────────────────────────────────────────────────────────
  // GET /admin/reports → ReportsController
  getAllReports(): Observable<any[]> {
    return this.api.get<any[]>('admin/reports');
  }

  // GET /admin/reports/:reportId → ReportsController
  getReport(reportId: string): Observable<any> {
    return this.api.get<any>(`admin/reports/${reportId}`);
  }

  // POST /admin/reports → ReportsController
  generateReport(requestData: any): Observable<any> {
    return this.api.post<any>('admin/reports', requestData);
  }

  // ── Audit Logs ───────────────────────────────────────────────────────────────
  // GET /admin/audit-logs → AuditLogsController
  getAuditLogs(page: number = 1, pageSize: number = 20): Observable<any> {
    return this.api.get<any>(`admin/audit-logs?page=${page}&pageSize=${pageSize}`);
  }
}
