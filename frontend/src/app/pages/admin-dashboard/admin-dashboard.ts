import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { ClaimService } from '../../core/services/claim.service';
import { AuthService } from '../../core/services/auth.service';
import { PolicyService } from '../../core/services/policy.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard implements OnInit {
  stats: any = {
    totalUsers: 0,
    totalPolicies: 0,
    totalClaims: 0,
    pendingClaims: 0,
    approvedClaims: 0,
    rejectedClaims: 0,
    activePolicies: 0,
    totalRevenue: 0
  };

  allUsers: any[] = [];
  allPolicies: any[] = [];
  recentPolicies: any[] = [];
  allClaims: any[] = [];
  recentClaims: any[] = [];
  loading = false;

  // Selected claim for modal review
  selectedClaim: any = null;
  selectedClaimDocuments: any[] = [];
  loadingDocuments = false;
  showReviewModal = false;

  // Review form fields
  reviewNotes = '';
  rejectionReason = '';
  approvedAmount: number | null = null;
  reviewAction: 'approve' | 'reject' | null = null;
  actionProcessing = false;
  actionMessage = '';

  // Tab navigation
  activeTab: 'overview' | 'claims' | 'policies' | 'users' | 'reports' = 'overview';

  // Audit and Reports
  auditLogs: any[] = [];
  reports: any[] = [];
  reportTitle = '';
  reportType = 'Financial';

  constructor(
    private adminService: AdminService,
    private claimService: ClaimService,
    private policyService: PolicyService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.fetchDashboardData();
    this.loadPlans();
    this.loadAuditLogs();
  }

  fetchDashboardData() {
    this.loading = true;

    forkJoin({
      stats: this.adminService.getDashboardStats().pipe(catchError(() => of(null))),
      users: this.adminService.getAllUsers().pipe(catchError(() => of([]))),
      policies: this.adminService.getAllPolicies().pipe(catchError(() => of([]))),
      claims: this.adminService.getAllClaims().pipe(catchError(() => of([]))),
      reports: this.adminService.getAllReports().pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ stats, users, policies, claims, reports }) => {
        if (stats) this.stats = { ...this.stats, ...stats };

        this.allUsers = Array.isArray(users) ? users : [];
        this.stats.totalUsers = this.allUsers.length;

        const pList = Array.isArray(policies) ? policies : [];
        this.allPolicies = pList;
        this.recentPolicies = pList.slice(0, 5);
        this.stats.totalPolicies = pList.length;
        this.stats.activePolicies = pList.filter((p: any) => p.status === 'Active').length;
        this.stats.totalRevenue = pList.reduce((sum: number, p: any) => sum + (p.premiumAmount || 0), 0);

        const cList = Array.isArray(claims) ? claims : [];
        this.allClaims = cList;
        this.recentClaims = cList.slice(0, 5);
        this.stats.totalClaims = cList.length;
        this.stats.pendingClaims = cList.filter((c: any) => c.status === 'Submitted').length;
        this.stats.approvedClaims = cList.filter((c: any) => c.status === 'Approved').length;
        this.stats.rejectedClaims = cList.filter((c: any) => c.status === 'Rejected').length;

        this.reports = Array.isArray(reports) ? reports : [];

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadPlans() {
    this.policyService.getInsuranceTypes().subscribe(data => {
      this.insuranceTypesList = Array.isArray(data) ? data : [];
    });
    this.policyService.getInsuranceSubtypes().subscribe(data => {
      this.insuranceSubtypesList = Array.isArray(data) ? data : [];
    });
    this.loadDiscounts();
  }

  loadDiscounts() {
    this.policyService.getDiscounts().subscribe({
      next: (data) => this.discountsList = Array.isArray(data) ? data : [],
      error: (err) => console.error('Error fetching discounts', err)
    });
  }

  openDiscountModal() {
    this.discountForm = { code: '', description: '', percentage: 10, maxDiscountAmount: 0, isFirstTimeOnly: false };
    this.discountActionMessage = '';
    this.showDiscountModal = true;
  }

  closeDiscountModal() {
    this.showDiscountModal = false;
  }

  submitDiscount() {
    if (!this.discountForm.code || !this.discountForm.percentage) {
      this.discountActionMessage = 'Code and percentage are required.';
      return;
    }
    
    // Ensure data meets backend validation
    const payload = {
        code: this.discountForm.code,
        percentage: Number(this.discountForm.percentage) || 10,
        description: this.discountForm.description?.trim() || `Discount for ${this.discountForm.code}`,
        maxDiscountAmount: Number(this.discountForm.maxDiscountAmount) || 0,
        isFirstTimeOnly: !!this.discountForm.isFirstTimeOnly
    };

    this.discountActionProcessing = true;
    this.policyService.createDiscount(payload).subscribe({
       next: () => {
         this.discountActionMessage = 'Success! Discount created.';
         this.loadDiscounts();
         setTimeout(() => {
           this.closeDiscountModal();
           this.discountActionProcessing = false;
         }, 1000);
       },
       error: (err) => {
         console.error('Submit discount error:', err);
         this.discountActionMessage = err?.error?.title || err?.error?.message || 'Failed to create discount. Check inputs.';
         this.discountActionProcessing = false;
       }
    });
  }

  deleteDiscount(id: string) {
    if (confirm('Are you sure you want to deactivate this discount code?')) {
       this.policyService.deleteDiscount(id).subscribe({
         next: () => this.loadDiscounts()
       });
    }
  }

  loadAuditLogs() {
    this.adminService.getAuditLogs(1, 50).subscribe({
      next: (data) => {
        this.auditLogs = Array.isArray(data) ? data : (data?.items || []);
      }
    });
  }

  deactivateUser(userId: string) {
    if (confirm('Are you sure you want to deactivate this user? This cannot be undone.')) {
      this.adminService.deactivateUser(userId).subscribe({
        next: () => {
          this.allUsers = this.allUsers.filter(u => u.userId !== userId && u.id !== userId);
          this.stats.totalUsers = this.allUsers.length;
          this.cdr.detectChanges();
          alert('User deactivated successfully.');
        },
        error: (err) => alert('Failed to deactivate user: ' + (err.error?.message || err.message))
      });
    }
  }

  assignAdminRole(userId: string) {
    if (confirm('Are you sure you want to promote this user to Admin?')) {
      // Assuming Admin role ID is "Admin" or you pass what Identity service expects
      this.authService.assignRole(userId, 'Admin').subscribe({
        next: () => {
          const user = this.allUsers.find(u => u.id === userId || u.userId === userId);
          if (user) user.role = 'Admin';
          alert('User successfully promoted to Admin.');
          this.cdr.detectChanges();
        },
        error: (err) => alert('Promotion failed: ' + (err.error?.message || err.message))
      });
    }
  }

  cancelPolicy(policyId: string, event?: Event) {
    if (event) event.stopPropagation();
    if (confirm('Are you sure you want to cancel this policy?')) {
      this.policyService.cancelPolicy(policyId).subscribe({
        next: () => {
          const policy = this.allPolicies.find(p => p.policyId === policyId);
          if (policy) policy.status = 'Cancelled';
          
          const recentPolicy = this.recentPolicies.find(p => p.policyId === policyId);
          if (recentPolicy) recentPolicy.status = 'Cancelled';

          this.stats.activePolicies = this.allPolicies.filter((p: any) => p.status === 'Active').length;
          this.cdr.detectChanges();
          alert('Policy cancelled successfully.');
        },
        error: (err) => alert('Failed to cancel policy: ' + (err.error?.message || err.message))
      });
    }
  } 

  // ── Plan Creation Modal ───────────────────────────────────────────────────

  showCreatePlanModal = false;
  creatingPlanType: 'type' | 'subtype' = 'type';
  insuranceTypesList: any[] = [];
  insuranceSubtypesList: any[] = [];
  
  // Custom discounts
  discountsList: any[] = [];
  showDiscountModal = false;
  discountForm: any = { code: '', description: '', percentage: 10, maxDiscountAmount: 0, isFirstTimeOnly: false };
  discountActionMessage = '';
  discountActionProcessing = false;


  planActionProcessing = false;
  planActionMessage = '';
  newPlanData = { name: '', description: '', typeId: '', basePremium: 0 };

  openCreatePlanModal(type: 'type' | 'subtype') {
    this.creatingPlanType = type;
    this.newPlanData = { name: '', description: '', typeId: '', basePremium: 0 };
    this.planActionMessage = '';
    this.showCreatePlanModal = true;
  }

  closeCreatePlanModal() {
    this.showCreatePlanModal = false;
  }

  submitNewPlan() {
    this.planActionProcessing = true;
    this.planActionMessage = '';

    if (this.creatingPlanType === 'type') {
      if (!this.newPlanData.name) {
         this.planActionMessage = 'Warning: Name is required.';
         this.planActionProcessing = false; return;
      }
      this.policyService.createInsuranceType({ name: this.newPlanData.name, description: this.newPlanData.description }).subscribe({
        next: (res) => {
          this.planActionMessage = 'Success: Insurance Type created successfully!';
          this.planActionProcessing = false;
          this.insuranceTypesList.push(res);
          setTimeout(() => this.closeCreatePlanModal(), 1500);
        },
        error: (err) => {
          this.planActionMessage = 'Failed: ' + (err.error?.message || err.message);
          this.planActionProcessing = false;
        }
      });
    } else {
      if (!this.newPlanData.name || !this.newPlanData.typeId) {
         this.planActionMessage = 'Warning: Name and Parent Type are required.';
         this.planActionProcessing = false; return;
      }
      
      const payload: any = {
        TypeId: this.newPlanData.typeId,
        Name: this.newPlanData.name,
        Description: this.newPlanData.description || '',
        BasePremium: this.newPlanData.basePremium ? Number(this.newPlanData.basePremium) : 0
      };

      this.policyService.createInsuranceSubtype(payload).subscribe({
        next: (res) => {
          this.planActionMessage = 'Success: Subtype Plan published successfully!';
          this.planActionProcessing = false;
          this.insuranceSubtypesList.push(res);
          this.cdr.detectChanges();
          setTimeout(() => this.closeCreatePlanModal(), 1500);
        },
        error: (err) => {
          console.error("Subtype Creation Failed:", err);
          this.planActionMessage = 'Failed: ' + (err.error?.message || err.message || 'Server rejected the request');
          this.planActionProcessing = false;
        }
      });
    }
  }

  removeInsuranceType(typeId: string) {
    if (confirm('Are you sure you want to delete this Master Insurance Type? This may affect associated plans.')) {
      this.policyService.deleteInsuranceType(typeId).subscribe({
        next: () => {
          this.insuranceTypesList = this.insuranceTypesList.filter(t => (t.insuranceTypeId || t.typeId || t.id) !== typeId);
          alert('Insurance Type deleted successfully.');
        },
        error: (err) => alert('Failed to delete: ' + (err.error?.message || err.message))
      });
    }
  }

  removeInsuranceSubtype(subtypeId: string) {
    if (confirm('Are you sure you want to delete this Specific Coverage Plan?')) {
      this.policyService.deleteInsuranceSubtype(subtypeId).subscribe({
        next: () => {
          this.insuranceSubtypesList = this.insuranceSubtypesList.filter(s => (s.subtypeId || s.id) !== subtypeId);
          alert('Coverage Plan deleted successfully.');
        },
        error: (err) => alert('Failed to delete: ' + (err.error?.message || err.message))
      });
    }
  }

  // ── Claim Review Modal ──────────────────────────────────────────────────────

  openReviewModal(claim: any) {
    this.selectedClaim = claim;
    this.selectedClaimDocuments = [];
    this.reviewNotes = '';
    this.rejectionReason = '';
    this.approvedAmount = claim.claimAmount || null;
    this.reviewAction = null;
    this.actionMessage = '';
    this.showReviewModal = true;
    this.loadingDocuments = true;
    this.cdr.detectChanges();

    // Fetch documents for this claim
    this.claimService.getClaimDocuments(claim.claimId).subscribe({
      next: (docs) => {
        this.selectedClaimDocuments = Array.isArray(docs) ? docs : [];
        this.loadingDocuments = false;
        this.cdr.detectChanges();
      },
      error: () => { 
        this.loadingDocuments = false; 
        this.cdr.detectChanges(); 
      }
    });
  }

  closeReviewModal() {
    this.showReviewModal = false;
    this.selectedClaim = null;
    this.selectedClaimDocuments = [];
    this.actionMessage = '';
    this.cdr.detectChanges();
  }

  setReviewAction(action: 'approve' | 'reject') {
    this.reviewAction = action;
    this.cdr.detectChanges();
  }

  submitReview() {
    if (!this.selectedClaim || !this.reviewAction) return;
    this.actionProcessing = true;
    this.actionMessage = '';

    const claimId = this.selectedClaim.claimId;

    if (this.reviewAction === 'approve') {
      this.adminService.approveClaim(claimId, this.approvedAmount ?? this.selectedClaim.claimAmount, this.reviewNotes).subscribe({
        next: () => {
          this.actionMessage = 'Success: Claim officially approved.';
          this.actionProcessing = false;
          this.selectedClaim.status = 'Approved';
          // Update list
          const c = this.allClaims.find(x => x.claimId === claimId);
          if (c) c.status = 'Approved';
          this.refreshStats();
          this.cdr.detectChanges();
          setTimeout(() => this.closeReviewModal(), 1000);
        },
        error: (err) => {
          this.actionMessage = 'Failed to approve: ' + (err.error?.message ?? err.message);
          this.actionProcessing = false;
          this.cdr.detectChanges();
        }
      });
    } else {
      if (!this.rejectionReason.trim()) {
        this.actionMessage = 'Warning: Please provide a structured rejection reason.';
        this.actionProcessing = false;
        return;
      }
      this.adminService.rejectClaim(claimId, this.rejectionReason).subscribe({
        next: () => {
          this.actionMessage = 'Success: Claim processing completely declined.';
          this.actionProcessing = false;
          this.selectedClaim.status = 'Rejected';
          const c = this.allClaims.find(x => x.claimId === claimId);
          if (c) c.status = 'Rejected';
          this.refreshStats();
          this.cdr.detectChanges();
          setTimeout(() => this.closeReviewModal(), 1000);
        },
        error: (err) => {
          this.actionMessage = 'Failed to reject: ' + (err.error?.message ?? err.message);
          this.actionProcessing = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  refreshStats() {
    this.stats.pendingClaims = this.allClaims.filter((c: any) => c.status === 'Submitted').length;
    this.stats.approvedClaims = this.allClaims.filter((c: any) => c.status === 'Approved').length;
    this.stats.rejectedClaims = this.allClaims.filter((c: any) => c.status === 'Rejected').length;
  }

  canReview(status: string): boolean {
    return status === 'Submitted' || status === 'Draft';
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'status-approved';
      case 'rejected': return 'status-rejected';
      case 'submitted': return 'status-submitted';
      case 'draft': return 'status-draft';
      default: return 'status-draft';
    }
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '—';
    const kb = bytes / 1024;
    return kb > 1024 ? `${(kb / 1024).toFixed(1)} MB` : `${kb.toFixed(1)} KB`;
  }

  generateReport() {
    this.actionProcessing = true;
    this.adminService.generateReport({
      title: this.reportTitle,
      reportType: this.reportType,
      format: 'PDF',
      dateRangeStart: new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString(),
      dateRangeEnd: new Date().toISOString()
    }).subscribe({
      next: (res) => {
        this.actionProcessing = false;
        alert('Report generated successfully!');
        if (Array.isArray(this.reports)) {
          this.reports.push(res);
        } else {
          this.reports = [res];
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.actionProcessing = false;
        alert('Failed: ' + (err.error?.message || err.message));
      }
    });
  }

  viewReport(report: any) {
    if (report.content) {
       try {
          const parsed = typeof report.content === 'string' ? JSON.parse(report.content) : report.content;
          if (parsed.statistics) {
             alert(`Report: ${parsed.title}\nTotal Insurance Sell: ${parsed.statistics.totalInsuranceSell}\nTotal Claim Accepted: ${parsed.statistics.totalClaimAccepted}\nTotal Claim Rejected: ${parsed.statistics.totalClaimRejected}`);
          } else {
             alert(JSON.stringify(parsed, null, 2));
          }
       } catch (e) {
          alert(report.content);
       }
    }
  }
}
