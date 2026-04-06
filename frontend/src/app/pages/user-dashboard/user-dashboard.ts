import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PolicyService } from '../../core/services/policy.service';
import { ClaimService } from '../../core/services/claim.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { PdfService } from '../../core/services/pdf.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-dashboard.html',
  styleUrl: './user-dashboard.css',
})
export class UserDashboard implements OnInit {
  policies: any[] = [];
  claims: any[] = [];
  insuranceTypes: any[] = [];
  profile: any = {};
  loading = true;

  activeTab: 'overview' | 'policies' | 'claims' = 'overview';

  // Pagination config
  policyPage = 1;
  claimPage = 1;
  pageSize = 5;

  get paginatedPolicies() {
    const start = (this.policyPage - 1) * this.pageSize;
    return this.policies.slice(start, start + this.pageSize);
  }

  get paginatedClaims() {
    const start = (this.claimPage - 1) * this.pageSize;
    return this.claims.slice(start, start + this.pageSize);
  }

  nextPolicyPage() { if (this.policyPage * this.pageSize < this.policies.length) this.policyPage++; }
  prevPolicyPage() { if (this.policyPage > 1) this.policyPage--; }
  
  nextClaimPage() { if (this.claimPage * this.pageSize < this.claims.length) this.claimPage++; }
  prevClaimPage() { if (this.claimPage > 1) this.claimPage--; }

  // Profile Form - removed (now in separate profile page)
  passwordData = { currentPassword: '', newPassword: '' };
  profileMessage = '';
  passwordMessage = '';

  // Policy Form
  showBuyPolicyModal = false;
  buyPolicyStep = 1;
  selectedType: string = '';
  subtypes: any[] = [];
  policyForm: any = { subtypeId: '', duration: 12 };
  homeDetails: any = { address: '', propertyType: 'House', yearBuilt: new Date().getFullYear(), estimatedValue: 0, securityFeatures: '' };
  vehicleDetails: any = { registrationNumber: '', make: '', model: '', manufactureYear: new Date().getFullYear(), estimatedValue: 0, chassisNumber: '', engineNumber: '' };
  policyMessage = '';
  calculatedPremium = 0;
  calculatedIdv = 0;
  quoteBreakdown = '';
  paymentProcessing = false;
  quoteLoading = false;
  selectedPlanName = '';
  paymentMethod = 'CreditCard';

  // Pending Actions
  pendingAction: string | null = null;
  pendingTypeId: string | null = null;
  pendingSubtypeId: string | null = null;

  // Discount
  couponCode = '';
  discountResult: any = null;
  applyingDiscount = false;
  
  // filtering
  vehicleMakeFilter = '';
  filteredSubtypes: any[] = [];
  uniqueVehicleMakes: string[] = [];
  availableYears: number[] = [];

  // Claim Form
  showClaimModal = false;
  claimForm: any = { policyId: '', description: '', claimAmount: 0, claimType: 'Accident', isCompletelyDamaged: false };
  claimMessage = '';
  selectedFile: File | null = null;
  selectedPolicyForClaim = '';
  claimProcessing = false;

  // Payment Form
  showPaymentModal = false;
  paymentForm = { amount: 0, paymentMethod: 'CreditCard' };
  paymentPolicyId = '';
  paymentMessage = '';

  constructor(
    private policyService: PolicyService,
    private claimService: ClaimService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private toastService: ToastService,
    private pdfService: PdfService
  ) {
    const currentYear = new Date().getFullYear();
    for (let i = 0; i <= 30; i++) {
       this.availableYears.push(currentYear - i);
    }
  }

  ngOnInit() {
    // Restore active tab from URL query params
    this.route.queryParams.subscribe(params => {
      const tab = params['tab'];
      if (tab && ['overview', 'policies', 'claims'].includes(tab)) {
        this.activeTab = tab;
      }

      this.pendingAction = params['action'] || null;
      this.pendingTypeId = params['typeId'] || null;
      this.pendingSubtypeId = params['subtypeId'] || null;
    });

    this.fetchData();
  }

  // Method to change tab and update URL
  setActiveTab(tab: 'overview' | 'policies' | 'claims') {
    this.activeTab = tab;
    // Update URL without reloading the page
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: tab },
      queryParamsHandling: 'merge'
    });
  }

  copyToClipboard(text: string) {
    if (!text) return;
    navigator.clipboard.writeText(text).then(() => {
      this.toastService.success(`Copied ID to clipboard: ${text}`);
    }).catch(err => {
      console.error('Failed to copy: ', err);
    });
  }

  fetchData() {
    this.loading = true;

    forkJoin({
      profile: this.authService.getProfile().pipe(catchError(() => of({}))),
      policies: this.policyService.getUserPolicies().pipe(catchError(() => of([]))),
      claims: this.claimService.getUserClaims().pipe(catchError(() => of([]))),
      types: this.policyService.getInsuranceTypes().pipe(catchError(() => of([])))
    }).subscribe({
      next: (result: any) => {
        const profileData = result.profile || {};
        
        this.profile = {
          fullName: profileData.fullName || profileData.FullName || '',
          phoneNumber: profileData.phoneNumber || profileData.PhoneNumber || '',
          address: profileData.address || profileData.Address || '',
          email: profileData.email || profileData.Email || ''
        };
        
        // Extract first name for welcome message
        const nameParts = this.profile.fullName.trim().split(' ');
        this.profile.firstName = nameParts[0] || '';
        
        this.policies = this.extractData(result.policies);
        this.claims = this.extractData(result.claims);
        const rawTypes = this.extractData(result.types);
        this.insuranceTypes = rawTypes.filter((t: any) => {
          const name = t.name || t.Name || '';
          return name.toLowerCase().includes('vehicle') || name.toLowerCase().includes('home');
        });
        this.loading = false;
        this.cdr.detectChanges();

        // Process pending action
        if (this.pendingAction === 'buy') {
          this.openBuyPolicyModal();
          if (this.pendingTypeId) {
            this.selectedType = this.pendingTypeId;
            this.policyForm.subtypeId = this.pendingSubtypeId || '';
            this.loadSubtypes();
          }
          
          this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { action: null, typeId: null, subtypeId: null },
            queryParamsHandling: 'merge'
          });
        }
      },
      error: (err: any) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  private extractData(data: any): any[] {
    if (!data) return [];
    if (Array.isArray(data)) return data;

    if (data.items) {
      if (Array.isArray(data.items)) return data.items;
      if (data.items.$values && Array.isArray(data.items.$values)) return data.items.$values;
    }
    if (data.Items) {
      if (Array.isArray(data.Items)) return data.Items;
      if (data.Items.$values && Array.isArray(data.Items.$values)) return data.Items.$values;
    }

    if (data.$values && Array.isArray(data.$values)) return data.$values;
    return [];
  }

  // --- Profile Management - Removed (now in separate profile page) ---

  // --- Buy Policy Flow ---
  openBuyPolicyModal() {
    this.buyPolicyStep = 1;
    this.policyMessage = '';
    this.policyForm = { subtypeId: '', duration: 12, nomineeName: '', nomineeRelation: '' };
    this.homeDetails = { address: '', propertyType: 'House', yearBuilt: new Date().getFullYear(), estimatedValue: 0, securityFeatures: '' };
    this.vehicleDetails = { registrationNumber: '', make: '', model: '', manufactureYear: new Date().getFullYear(), estimatedValue: 0, chassisNumber: '', engineNumber: '' };
    this.calculatedPremium = 0;
    this.calculatedIdv = 0;
    this.quoteBreakdown = '';
    this.vehicleMakeFilter = '';
    this.filteredSubtypes = [];
    this.showBuyPolicyModal = true;
  }

  loadSubtypes() {
    if (!this.selectedType) return;
    this.policyService.getSubtypesByType(this.selectedType).subscribe({
      next: (data: any) => {
        this.subtypes = this.extractData(data);
        this.extractUniqueVehicleMakes();
        this.filterSubtypes();
        this.updateSelectedPlanName();
      }
    });
  }

  filterSubtypes() {
    if (this.isVehicleType() && this.vehicleMakeFilter) {
      const filterLower = this.vehicleMakeFilter.toLowerCase().trim();
      this.filteredSubtypes = this.subtypes.filter(s => (s.name || s.Name || '').toLowerCase().startsWith(filterLower) || (s.name || s.Name || '').toLowerCase().includes(filterLower));
    } else {
      this.filteredSubtypes = this.subtypes;
    }
  }

  extractUniqueVehicleMakes() {
    if (this.isVehicleType() && this.subtypes.length > 0) {
      const makes = new Set<string>();
      this.subtypes.forEach(s => {
        const name = s.name || s.Name || '';
        // Extract the first word as the make (e.g., "Maruti" from "Maruti Suzuki Comprehensive")
        const firstWord = name.split(' ')[0];
        if (firstWord) {
          makes.add(firstWord);
        }
      });
      this.uniqueVehicleMakes = Array.from(makes).sort();
    } else {
      this.uniqueVehicleMakes = [];
    }
  }

  onVehicleMakeChange() {
     this.vehicleDetails.make = this.vehicleMakeFilter;
     this.filterSubtypes();
     this.updateSelectedPlanName();
  }

  updateSelectedPlanName() {
    const subtype = this.subtypes.find(s => (s.subtypeId || s.SubtypeId) === this.policyForm.subtypeId);
    if (subtype) {
      this.selectedPlanName = subtype.name || subtype.Name;
    }
  }

  isVehicleType(): boolean {
    const typeObj = this.insuranceTypes.find(t => (t.id || t.typeId || t.TypeId) === this.selectedType);
    const name = typeObj?.name || typeObj?.Name || '';
    return name.toLowerCase().includes('vehicle') || false;
  }

  isHomeType(): boolean {
    const typeObj = this.insuranceTypes.find(t => (t.id || t.typeId || t.TypeId) === this.selectedType);
    const name = typeObj?.name || typeObj?.Name || '';
    return name.toLowerCase().includes('home') || false;
  }

  // Add formSubmitted flag
  formSubmitted = false;

  proceedToReview() {
    this.formSubmitted = true;
    
    // Nominee fields are now optional, so remove this validation
    // if (!this.policyForm.nomineeName || !this.policyForm.nomineeRelation) {
    //   this.policyMessage = 'Please enter nominee details.';
    //   return;
    // }
    
    if (this.isHomeType() && (!this.homeDetails.address || !this.homeDetails.propertyType || !this.homeDetails.estimatedValue || this.homeDetails.estimatedValue <= 0)) {
      this.policyMessage = 'Please fill all required home details.';
      return;
    }
    if (this.isVehicleType() && (!this.vehicleDetails.registrationNumber || !this.vehicleDetails.make || !this.vehicleDetails.model || !this.vehicleDetails.chassisNumber || !this.vehicleDetails.engineNumber || !this.vehicleDetails.estimatedValue || this.vehicleDetails.estimatedValue <= 0)) {
      this.policyMessage = 'Please fill all required vehicle details including chassis and engine numbers.';
      return;
    }
    if (this.isVehicleType()) {
      const plateRegex = /^[A-Z]{2}[ -]?[0-9]{1,2}[ -]?[A-Z]{1,3}[ -]?[0-9]{4}$/i;
      if (!plateRegex.test(this.vehicleDetails.registrationNumber)) {
        this.policyMessage = 'Registration Number must be in a valid format (e.g. MH-12-AB-1234 or MH12AB1234)';
        return;
      }
      if (this.vehicleDetails.chassisNumber.length < 10) {
        this.policyMessage = 'Chassis Number must be at least 10 characters long.';
        return;
      }
      if (this.vehicleDetails.engineNumber.length < 6) {
        this.policyMessage = 'Engine Number must be at least 6 characters long.';
        return;
      }
    }

    this.policyMessage = '';
    this.formSubmitted = false; // Reset after validation passes
    this.quoteLoading = true;
    this.buyPolicyStep = 3;

    // Reset discount
    this.couponCode = '';
    this.discountResult = null;

    // Call the backend to calculate IDV and premium
    const quotePayload: any = {
      subtypeId: this.policyForm.subtypeId,
      duration: this.policyForm.duration,
    };

    if (this.isHomeType()) {
      quotePayload.homeDetail = Object.assign({}, this.homeDetails);
    }

    if (this.isVehicleType()) {
      quotePayload.vehicleDetail = Object.assign({}, this.vehicleDetails);
      if (!quotePayload.vehicleDetail.chassisNumber) quotePayload.vehicleDetail.chassisNumber = 'CH' + Math.random().toString(36).substring(7).toUpperCase();
      if (!quotePayload.vehicleDetail.engineNumber) quotePayload.vehicleDetail.engineNumber = 'EN' + Math.random().toString(36).substring(7).toUpperCase();
    }

    this.policyService.calculateQuote(quotePayload).subscribe({
      next: (quote) => {
        this.calculatedIdv = quote.insuredDeclaredValue || quote.InsuredDeclaredValue || 0;
        this.calculatedPremium = quote.premiumAmount || quote.PremiumAmount || 0;
        this.quoteBreakdown = quote.breakdown || quote.Breakdown || '';
        
        // After getting original premium, get the discount!
        this.fetchDiscountData();
      },
      error: (err) => {
        this.quoteLoading = false;
        const subtypeId = this.policyForm.subtypeId;
        const subtype = this.subtypes.find(s => (s.subtypeId || s.SubtypeId) === subtypeId);
        const basePremium = subtype?.basePremium || subtype?.BasePremium || 0;
        this.calculatedPremium = basePremium * (this.policyForm.duration / 12);
        this.calculatedIdv = this.isVehicleType() ? this.vehicleDetails.estimatedValue : this.homeDetails.estimatedValue;
        this.quoteBreakdown = 'Estimated values (detailed calculation unavailable)';
        
        // Fetch discounts even on simple fallback
        this.fetchDiscountData();
      }
    });
  }

  fetchDiscountData() {
    this.applyingDiscount = true;
    this.policyService.calculateDiscount(this.calculatedPremium, this.couponCode).subscribe({
       next: (res) => {
         this.discountResult = res;
         this.applyingDiscount = false;
         this.quoteLoading = false;
         this.cdr.detectChanges();
       },
       error: (err) => {
         this.applyingDiscount = false;
         this.quoteLoading = false;
         this.policyMessage = 'Error calculating discounts. Check if coupon is valid.';
         this.cdr.detectChanges();
       }
    });
  }

  applyCoupon() {
    if (!this.couponCode) return;
    this.fetchDiscountData();
  }

  proceedToPayment() {
    this.buyPolicyStep = 4;
  }

  submitPolicy() {
    this.paymentProcessing = true;
    this.policyMessage = 'Creating policy and processing payment...';

    const payload: any = {
      subtypeId: this.policyForm.subtypeId,
      duration: this.policyForm.duration,
      couponCode: this.couponCode || null,
      nomineeName: this.policyForm.nomineeName,
      nomineeRelation: this.policyForm.nomineeRelation
    };

    if (this.isHomeType()) {
      payload.homeDetail = {
        address: this.homeDetails.address,
        propertyType: this.homeDetails.propertyType || 'Residential',
        yearBuilt: this.homeDetails.yearBuilt || 2020,
        estimatedValue: this.homeDetails.estimatedValue,
        securityFeatures: this.homeDetails.securityFeatures || 'Standard'
      };
    }

    if (this.isVehicleType()) {
      payload.vehicleDetail = {
        registrationNumber: this.vehicleDetails.registrationNumber,
        make: this.vehicleDetails.make,
        model: this.vehicleDetails.model,
        manufactureYear: this.vehicleDetails.manufactureYear || 2022,
        estimatedValue: this.vehicleDetails.estimatedValue,
        chassisNumber: this.vehicleDetails.chassisNumber || 'CH' + Math.random().toString(36).substring(7).toUpperCase(),
        engineNumber: this.vehicleDetails.engineNumber || 'EN' + Math.random().toString(36).substring(7).toUpperCase()
      };
    }

    // Step 1: Create the policy (status = Pending)
    this.policyService.buyPolicy(payload).subscribe({
      next: (res: any) => {
        this.policyMessage = 'Policy created! Processing payment...';
        const pId = res.policyId || res.PolicyId;
        const pAmount = res.premiumAmount || res.PremiumAmount;
        const createdPolicy = res; // Store the created policy
        
        // Step 2: Process payment (this will also activate the policy in backend)
        this.policyService.processPayment(pId, {
          policyId: pId,
          amount: pAmount,
          paymentMethod: this.paymentMethod
        }).subscribe({
          next: () => {
            this.paymentProcessing = false;
            this.buyPolicyStep = 5; // Success Step
            this.toastService.success('Policy purchased successfully!');
            
            // Generate and download PDF invoice
            const policyDetails = this.isVehicleType() ? this.vehicleDetails : this.homeDetails;
            this.pdfService.generatePolicyInvoice(createdPolicy, policyDetails);
            
            this.fetchData();
          },
          error: (err) => {
            this.paymentProcessing = false;
            this.policyMessage = 'Policy created, but payment failed: ' + (err.error?.message || err.message);
            this.toastService.error('Payment failed: ' + (err.error?.message || err.message));
          }
        });
      },
      error: (err) => {
        this.paymentProcessing = false;
        this.policyMessage = 'Failed to create policy: ' + (err.error?.message || err.message);
        this.toastService.error('Failed to create policy: ' + (err.error?.message || err.message));
      }
    });
  }

  // --- Payments ---
  openPaymentModal(policy: any) {
    this.paymentPolicyId = policy.policyId || policy.PolicyId;
    this.paymentForm.amount = policy.premiumAmount || policy.PremiumAmount || 0;
    this.paymentMessage = '';
    this.showPaymentModal = true;
  }

  submitPayment() {
    this.policyService.processPayment(this.paymentPolicyId, {
      policyId: this.paymentPolicyId,
      amount: this.paymentForm.amount,
      paymentMethod: this.paymentForm.paymentMethod
    }).subscribe({
      next: () => {
        this.paymentMessage = 'Success: Payment processed & policy activated.';
        this.fetchData();
        setTimeout(() => this.showPaymentModal = false, 1500);
      },
      error: (err) => this.paymentMessage = 'Failed: ' + (err.error?.message || err.message)
    });
  }

  // --- Claims Flow ---
  openClaimModal() {
    this.claimForm = { policyId: '', description: '', claimAmount: 0, claimType: 'Accident', isCompletelyDamaged: false };
    this.claimMessage = '';
    this.selectedFile = null;
    this.showClaimModal = true;
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  submitClaim() {
    // Prevent double submission
    if (this.claimProcessing) return;
    
    if (!this.claimForm.policyId || !this.claimForm.description) {
      this.claimMessage = 'Please fill all required fields.';
      this.cdr.detectChanges();
      return;
    }
    
    this.claimProcessing = true;
    this.claimMessage = 'Processing your claim...';
    this.cdr.detectChanges();
    
    const selectedPolicy = this.policies.find(p => p.policyId === this.claimForm.policyId);
    if (!selectedPolicy) return;
    
    // Check IDV Limit and Conditions
    if (this.isPolicyVehicleType(selectedPolicy)) {
       if (this.claimForm.claimType === 'Stolen' || this.claimForm.claimType === 'NaturalDisaster') {
          if (this.claimForm.claimAmount > selectedPolicy.insuredDeclaredValue) {
            this.claimMessage = 'For Stolen/Natural Disaster, max claim is IDV (₹' + selectedPolicy.insuredDeclaredValue + ')';
            this.claimProcessing = false;
            this.cdr.detectChanges();
            return;
          }
       } else if (this.claimForm.claimType === 'Accident') {
          if (this.claimForm.isCompletelyDamaged) {
             if (!this.selectedFile) {
               this.claimMessage = 'Please upload a document/image for full damage claim.';
               this.claimProcessing = false;
               this.cdr.detectChanges();
               return;
             }
             this.claimForm.claimAmount = selectedPolicy.insuredDeclaredValue;
          } else {
             const maxAccidentAmount = selectedPolicy.insuredDeclaredValue * 0.75;
             if (this.claimForm.claimAmount > maxAccidentAmount) {
                this.claimMessage = 'For Accident (repairable), you can only claim up to 75% of IDV (₹' + maxAccidentAmount + ')';
                this.claimProcessing = false;
                this.cdr.detectChanges();
                return;
             }
          }
       }
    } else {
       if (this.claimForm.claimAmount > selectedPolicy.insuredDeclaredValue) {
          this.claimMessage = 'Please Enter Amount less than or equal to IDV (₹' + selectedPolicy.insuredDeclaredValue + ')';
          this.claimProcessing = false;
          this.cdr.detectChanges();
          return;
       }
    }
    
    this.claimService.submitClaim(this.claimForm).subscribe({
      next: (res: any) => {
        const cId = res.claimId || res.ClaimId;
        // Upload document if selected
        if (this.selectedFile) {
          this.claimMessage = 'Uploading document...';
          this.cdr.detectChanges();
          const formData = new FormData();
          formData.append('file', this.selectedFile);
          this.claimService.uploadDocument(cId, formData).subscribe({
            next: () => {
               this.claimMessage = 'Submitting to review...';
               this.cdr.detectChanges();
               this.claimService.submitClaimToReview(cId).subscribe({
                 next: () => {
                   this.claimMessage = 'Success: Claim and document submitted to review.';
                   this.toastService.success('Claim and document submitted successfully!');
                   this.cdr.detectChanges();
                   this.fetchData();
                   setTimeout(() => {
                     this.showClaimModal = false;
                     this.claimMessage = '';
                     this.resetClaimForm();
                     this.claimProcessing = false;
                     this.cdr.detectChanges();
                   }, 2000);
                 },
                 error: (err) => {
                   const errorMessage = err.error?.message || err.message || 'Failed to submit claim';
                   this.claimMessage = 'Error: ' + errorMessage;
                   this.toastService.error(errorMessage, 5000);
                   this.claimProcessing = false;
                   this.cdr.detectChanges();
                 }
               });
            },
            error: (err) => {
              const errorMessage = err.error?.message || err.message || 'Document upload failed';
              this.claimMessage = 'Document upload failed: ' + errorMessage;
              this.toastService.error(errorMessage, 5000);
              this.claimProcessing = false;
              this.cdr.detectChanges();
            }
          });
        } else {
           this.claimMessage = 'Submitting to review...';
           this.cdr.detectChanges();
           this.claimService.submitClaimToReview(cId).subscribe({
             next: () => {
               this.claimMessage = 'Success: Claim submitted to review.';
               this.toastService.success('Claim submitted successfully!');
               this.cdr.detectChanges();
               this.fetchData();
               setTimeout(() => {
                 this.showClaimModal = false;
                 this.claimMessage = '';
                 this.resetClaimForm();
                 this.claimProcessing = false;
                 this.cdr.detectChanges();
               }, 2000);
             },
             error: (err) => {
               const errorMessage = err.error?.message || err.message || 'Failed to submit claim';
               this.claimMessage = 'Error: ' + errorMessage;
               this.toastService.error(errorMessage, 5000);
               this.claimProcessing = false;
               this.cdr.detectChanges();
             }
           });
        }
      },
      error: (err) => {
        const errorMessage = err.error?.message || err.message || 'Failed to submit claim';
        this.claimMessage = 'Error: ' + errorMessage;
        this.toastService.error(errorMessage, 5000);
        this.claimProcessing = false;
        this.cdr.detectChanges();
      }
    });
  }

  // --- Helpers ---
  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'active': return 'status-active';
      case 'pending': return 'status-pending';
      case 'cancelled': return 'status-cancelled';
      case 'expired': return 'status-expired';
      default: return '';
    }
  }

  isPolicyVehicleType(policy: any): boolean {
    if (!policy) return false;
    const name = policy.typeName?.toLowerCase() || '';
    return name.includes('vehicle') || name.includes('auto') || name.includes('car');
  }

  downloadPolicyInvoice(policy: any) {
    // Fetch policy details (vehicle or home)
    const policyId = policy.policyId || policy.PolicyId;
    
    if (this.isPolicyVehicleType(policy)) {
      // Fetch vehicle details
      this.policyService.getVehicleDetail(policyId).subscribe({
        next: (details: any) => {
          this.pdfService.generatePolicyInvoice(policy, details);
          this.toastService.success('Invoice generated successfully!');
        },
        error: (err: any) => {
          this.toastService.error('Failed to generate invoice: ' + (err.error?.message || err.message));
        }
      });
    } else {
      // Fetch home details
      this.policyService.getHomeDetail(policyId).subscribe({
        next: (details: any) => {
          this.pdfService.generatePolicyInvoice(policy, details);
          this.toastService.success('Invoice generated successfully!');
        },
        error: (err: any) => {
          this.toastService.error('Failed to generate invoice: ' + (err.error?.message || err.message));
        }
      });
    }
  }

  onClaimPolicyChange() {
    this.claimForm.claimAmount = 0;
    this.claimForm.claimType = 'Accident';
    this.claimForm.isCompletelyDamaged = false;
  }

  getSelectedClaimPolicy() {
    if (!this.claimForm.policyId) return null;
    return this.policies.find(p => p.policyId === this.claimForm.policyId);
  }

  onCompletelyDamagedChange() {
    const selectedPolicy = this.policies.find(p => p.policyId === this.claimForm.policyId);
    if (this.claimForm.isCompletelyDamaged && selectedPolicy) {
       this.claimForm.claimAmount = selectedPolicy.insuredDeclaredValue;
    } else {
       this.claimForm.claimAmount = 0;
    }
  }

  resetClaimForm() {
    this.claimForm = { policyId: '', description: '', claimAmount: 0, claimType: 'Accident', isCompletelyDamaged: false };
    this.selectedFile = null;
  }
}
