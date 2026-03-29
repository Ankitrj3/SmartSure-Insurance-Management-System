import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { ClaimService } from '../../core/services/claim.service';
import { AuthService } from '../../core/services/auth.service';
import { PolicyService } from '../../core/services/policy.service';
import { forkJoin, of } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

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
  reviewAction: 'approve' | 'reject' | 'underReview' | 'closed' | null = null;
  actionProcessing = false;
  actionMessage = '';

  // Tab navigation
  activeTab: 'overview' | 'claims' | 'policies' | 'users' | 'reports' = 'overview';

  // Audit and Reports
  auditLogs: any[] = [];
  reports: any[] = [];
  reportTitle = '';
  reportType = 'Financial';

  // Show More pagination state
  showAllClaims = false;
  showAllReports = false;

  get visibleClaims(): any[] {
    return this.showAllClaims ? this.allClaims : this.allClaims.slice(0, 6);
  }

  get visibleReports(): any[] {
    return this.showAllReports ? this.reports : this.reports.slice(0, 6);
  }

  constructor(
    private adminService: AdminService,
    private claimService: ClaimService,
    private policyService: PolicyService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    // Restore active tab from URL query params
    this.route.queryParams.subscribe(params => {
      const tab = params['tab'];
      if (tab && ['overview', 'claims', 'policies', 'users', 'reports'].includes(tab)) {
        this.activeTab = tab;
      }
    });

    this.fetchDashboardData();
    this.loadPlans();
    this.loadAuditLogs();
  }

  // Method to change tab and update URL
  setActiveTab(tab: 'overview' | 'claims' | 'policies' | 'users' | 'reports') {
    this.activeTab = tab;
    // Update URL without reloading the page
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: tab },
      queryParamsHandling: 'merge'
    });
  }

  fetchDashboardData() {
    this.loading = true;
    this.cdr.detectChanges();

    forkJoin({
      stats: this.adminService.getDashboardStats().pipe(catchError(() => of(null))),
      users: this.adminService.getAllUsers().pipe(catchError(() => of([]))),
      policies: this.adminService.getAllPolicies().pipe(catchError(() => of([]))),
      claims: this.adminService.getAllClaims().pipe(catchError((err) => {
        console.error('Error fetching claims:', err);
        return of([]);
      })),
      reports: this.adminService.getAllReports().pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ stats, users, policies, claims, reports }) => {
        if (stats) this.stats = { ...this.stats, ...stats };

        // Map users and extract role from roles array
        this.allUsers = Array.isArray(users) ? users.map(u => ({
          ...u,
          role: u.roles && u.roles.length > 0 ? (u.roles.includes('Admin') ? 'Admin' : u.roles[0]) : 'User'
        })) : [];
        this.stats.totalUsers = this.allUsers.length;

        const pList = Array.isArray(policies) ? policies : [];
        this.allPolicies = pList;
        this.recentPolicies = pList.slice(0, 5);
        this.stats.totalPolicies = pList.length;
        this.stats.activePolicies = pList.filter((p: any) => p.status === 'Active').length;
        this.stats.totalRevenue = pList.reduce((sum: number, p: any) => sum + (p.premiumAmount || 0), 0);

        // Extract claims data properly
        const claimsData: any = claims;
        let cList: any[] = [];
        if (Array.isArray(claimsData)) {
          cList = claimsData;
        } else if (claimsData && typeof claimsData === 'object') {
          if (claimsData.$values) {
            cList = claimsData.$values;
          } else if (claimsData.value) {
            cList = claimsData.value;
          } else if (claimsData.items) {
            cList = claimsData.items;
          }
        }
        
        this.allClaims = cList;
        this.recentClaims = cList.slice(0, 5);
        this.stats.totalClaims = cList.length;
        this.stats.pendingClaims = cList.filter((c: any) => {
          const status = c.status || c.Status;
          return status === 'Submitted';
        }).length;
        this.stats.approvedClaims = cList.filter((c: any) => {
          const status = c.status || c.Status;
          return status === 'Approved';
        }).length;
        this.stats.rejectedClaims = cList.filter((c: any) => {
          const status = c.status || c.Status;
          return status === 'Rejected';
        }).length;

        this.reports = Array.isArray(reports) ? reports : [];
      },
      error: (err) => {
        console.error('Dashboard data fetch error:', err);
      },
      complete: () => {
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
    console.log('Fetching documents for claim:', claim.claimId);
    this.claimService.getClaimDocuments(claim.claimId).subscribe({
      next: (response) => {
        console.log('Raw documents response:', response);
        
        // Handle different response formats
        let docsArray: any[] = [];
        
        if (Array.isArray(response)) {
          docsArray = response;
        } else if (response && typeof response === 'object') {
          const responseData: any = response;
          if (responseData.$values && Array.isArray(responseData.$values)) {
            docsArray = responseData.$values;
          } else if (responseData.value && Array.isArray(responseData.value)) {
            docsArray = responseData.value;
          }
        }
        
        console.log('Processed documents array:', docsArray);
        console.log('Documents count:', docsArray.length);
        
        this.selectedClaimDocuments = docsArray;
        this.loadingDocuments = false;
        
        // Force change detection
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading documents:', err);
        this.loadingDocuments = false;
        this.selectedClaimDocuments = [];
        this.cdr.detectChanges();
      }
    });
  }

  closeReviewModal() {
    this.showReviewModal = false;
    this.selectedClaim = null;
    this.selectedClaimDocuments = [];
    this.actionMessage = '';
  }

  setReviewAction(action: 'approve' | 'reject' | 'underReview' | 'closed') {
    this.reviewAction = action;
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
          const c = this.allClaims.find(x => x.claimId === claimId);
          if (c) c.status = 'Approved';
          this.refreshStats();
          
          // Close modal immediately
          this.showReviewModal = false;
          this.fetchDashboardData();
        },
        error: (err) => {
          this.actionMessage = 'Failed to approve: ' + (err.error?.message ?? err.message);
          this.actionProcessing = false;
        }
      });
    } else if (this.reviewAction === 'reject') {
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
          
          // Close modal immediately
          this.showReviewModal = false;
          this.fetchDashboardData();
        },
        error: (err) => {
          this.actionMessage = 'Failed to reject: ' + (err.error?.message ?? err.message);
          this.actionProcessing = false;
        }
      });
    } else if (this.reviewAction === 'underReview') {
      this.adminService.setClaimUnderReview(claimId, this.reviewNotes).subscribe({
        next: () => {
          this.actionMessage = 'Success: Claim set to Under Review.';
          this.actionProcessing = false;
          this.selectedClaim.status = 'UnderReview';
          const c = this.allClaims.find(x => x.claimId === claimId);
          if (c) c.status = 'UnderReview';
          this.refreshStats();
          
          // Close modal immediately
          this.showReviewModal = false;
          this.fetchDashboardData();
        },
        error: (err) => {
          this.actionMessage = 'Failed to set under review: ' + (err.error?.message ?? err.message);
          this.actionProcessing = false;
        }
      });
    } else if (this.reviewAction === 'closed') {
      this.adminService.setClaimClosed(claimId, this.reviewNotes).subscribe({
        next: () => {
          this.actionMessage = 'Success: Claim closed.';
          this.actionProcessing = false;
          this.selectedClaim.status = 'Closed';
          const c = this.allClaims.find(x => x.claimId === claimId);
          if (c) c.status = 'Closed';
          this.refreshStats();
          
          // Close modal immediately
          this.showReviewModal = false;
          this.fetchDashboardData();
        },
        error: (err) => {
          this.actionMessage = 'Failed to close claim: ' + (err.error?.message ?? err.message);
          this.actionProcessing = false;
        }
      });
    }
  }

  refreshStats() {
    this.stats.pendingClaims = this.allClaims.filter((c: any) => {
      const status = c.status || c.Status;
      return status === 'Submitted';
    }).length;
    this.stats.approvedClaims = this.allClaims.filter((c: any) => {
      const status = c.status || c.Status;
      return status === 'Approved';
    }).length;
    this.stats.rejectedClaims = this.allClaims.filter((c: any) => {
      const status = c.status || c.Status;
      return status === 'Rejected';
    }).length;
  }

  canReview(status: string): boolean {
    return status === 'Submitted' || status === 'Draft' || status === 'UnderReview';
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
    // Validate inputs
    if (!this.reportTitle || this.reportTitle.trim() === '') {
      alert('Please enter a report title');
      return;
    }
    
    if (!this.reportType || this.reportType.trim() === '') {
      alert('Please select a report type');
      return;
    }
    
    this.actionProcessing = true;
    this.adminService.generatePdfReport({
      title: this.reportTitle.trim(),
      reportType: this.reportType,
      format: 'PDF',
      dateRangeStart: new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString(),
      dateRangeEnd: new Date().toISOString()
    }).subscribe({
      next: (blob: Blob) => {
        this.actionProcessing = false;
        alert('PDF Report generated successfully!');
        
        // Background refresh reports table to get the newly stored report metadata
        this.adminService.getAllReports().subscribe(r => this.reports = Array.isArray(r) ? r : []);

        // Automatically download the real PDF
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${this.reportTitle.replace(/\s+/g, '_')}_${new Date().getTime()}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        // Clear the form
        this.reportTitle = '';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.actionProcessing = false;
        if (err.error instanceof Blob) {
          err.error.text().then((text: string) => {
            try {
              const errorObj = JSON.parse(text);
              alert('Failed to generate PDF: ' + (errorObj.message || errorObj.title || 'Internal Server Error'));
            } catch (e) {
              alert('Failed to generate PDF: ' + text);
            }
          });
        } else {
          alert('Failed to generate PDF: ' + (err.error?.message || err.message || 'Internal Server Error'));
        }
      }
    });
  }

  viewReport(report: any) {
    this.actionProcessing = true;
    this.adminService.generatePdfReport({
      title: report.title,
      reportType: report.reportType || report.type || 'Financial',
      format: 'PDF',
      dateRangeStart: report.dateRangeStart || new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString(),
      dateRangeEnd: report.dateRangeEnd || new Date().toISOString()
    }).subscribe({
      next: (blob: Blob) => {
        this.actionProcessing = false;
        const url = window.URL.createObjectURL(new Blob([blob], { type: 'application/pdf' }));
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 10000);
      },
      error: (err) => {
        this.actionProcessing = false;
        if (err.error instanceof Blob) {
          err.error.text().then((text: string) => {
            try {
              const errorObj = JSON.parse(text);
              alert('Failed to generate PDF: ' + (errorObj.message || errorObj.title || 'Internal Server Error'));
            } catch (e) {
              alert('Failed to generate PDF: ' + text);
            }
          });
        } else {
          alert('Failed to fetch and view PDF: ' + (err.error?.message || err.message));
        }
      }
    });
  }

  toggleShowMoreClaims() {
    this.showAllClaims = !this.showAllClaims;
  }

  toggleShowMoreReports() {
    this.showAllReports = !this.showAllReports;
  }

  deleteReport(report: any) {
    if (!confirm(`Are you sure you want to delete the report "${report.title}"? This cannot be undone.`)) {
      return;
    }
    this.adminService.deleteReport(report.reportId).subscribe({
      next: () => {
        this.reports = this.reports.filter(r => r.reportId !== report.reportId);
      },
      error: (err) => {
        alert('Failed to delete report: ' + (err.error?.message || err.message || 'Unknown error'));
      }
    });
  }
}
