import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PolicyService } from '../../core/services/policy.service';
import { ClaimService } from '../../core/services/claim.service';
import { AuthService } from '../../core/services/auth.service';
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

  activeTab: 'overview' | 'profile' | 'policies' | 'claims' = 'overview';

  // Profile Form
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

  // Discount
  couponCode = '';
  discountResult: any = null;
  applyingDiscount = false;

  // Claim Form
  showClaimModal = false;
  claimForm = { policyId: '', description: '', claimAmount: 0 };
  claimMessage = '';
  selectedFile: File | null = null;
  selectedPolicyForClaim = '';

  // Payment Form
  showPaymentModal = false;
  paymentForm = { amount: 0, paymentMethod: 'CreditCard' };
  paymentPolicyId = '';
  paymentMessage = '';

  constructor(
    private policyService: PolicyService,
    private claimService: ClaimService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.fetchData();
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
        this.profile = result.profile || {};
        this.policies = this.extractData(result.policies);
        this.claims = this.extractData(result.claims);
        const rawTypes = this.extractData(result.types);
        this.insuranceTypes = rawTypes.filter((t: any) => {
          const name = t.name || t.Name || '';
          return name.toLowerCase().includes('vehicle') || name.toLowerCase().includes('home');
        });
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  private extractData(data: any): any[] {
    if (Array.isArray(data)) return data;
    if (data && data.$values && Array.isArray(data.$values)) return data.$values;
    return [];
  }

  // --- Profile Management ---
  updateProfile() {
    this.authService.updateProfile({ 
      firstName: this.profile.firstName, 
      lastName: this.profile.lastName, 
      phoneNumber: this.profile.phoneNumber 
    }).subscribe({
      next: () => this.profileMessage = 'Success: Profile updated!',
      error: (err) => this.profileMessage = 'Failed to update: ' + (err.error?.message || err.message)
    });
  }

  changePassword() {
    this.authService.changePassword(this.passwordData).subscribe({
      next: () => {
        this.passwordMessage = 'Success: Password changed.';
        this.passwordData = { currentPassword: '', newPassword: '' };
      },
      error: (err) => this.passwordMessage = 'Failed to change password: ' + (err.error?.message || err.message)
    });
  }

  // --- Buy Policy Flow ---
  openBuyPolicyModal() {
    this.buyPolicyStep = 1;
    this.policyMessage = '';
    this.policyForm = { subtypeId: '', duration: 12 };
    this.homeDetails = { address: '', propertyType: 'House', yearBuilt: new Date().getFullYear(), estimatedValue: 0, securityFeatures: '' };
    this.vehicleDetails = { registrationNumber: '', make: '', model: '', manufactureYear: new Date().getFullYear(), estimatedValue: 0, chassisNumber: '', engineNumber: '' };
    this.calculatedPremium = 0;
    this.calculatedIdv = 0;
    this.quoteBreakdown = '';
    this.showBuyPolicyModal = true;
  }

  loadSubtypes() {
    if (!this.selectedType) return;
    this.policyService.getSubtypesByType(this.selectedType).subscribe({
      next: (data: any) => {
        this.subtypes = this.extractData(data);
        this.updateSelectedPlanName();
      }
    });
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

  proceedToReview() {
    if (this.isHomeType() && (!this.homeDetails.address || !this.homeDetails.propertyType || !this.homeDetails.estimatedValue)) {
      this.policyMessage = 'Please fill all required home details.';
      return;
    }
    if (this.isVehicleType() && (!this.vehicleDetails.registrationNumber || !this.vehicleDetails.make || !this.vehicleDetails.model || !this.vehicleDetails.estimatedValue)) {
      this.policyMessage = 'Please fill all required vehicle details.';
      return;
    }

    this.policyMessage = '';
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
      couponCode: this.couponCode
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
        // Step 2: Process payment (this will also activate the policy in backend)
        this.policyService.processPayment(pId, {
          policyId: pId,
          amount: pAmount,
          paymentMethod: this.paymentMethod
        }).subscribe({
          next: () => {
            this.paymentProcessing = false;
            this.buyPolicyStep = 5; // Success Step
            this.fetchData();
          },
          error: (err) => {
            this.paymentProcessing = false;
            this.policyMessage = 'Policy created, but payment failed: ' + (err.error?.message || err.message);
          }
        });
      },
      error: (err) => {
        this.paymentProcessing = false;
        this.policyMessage = 'Failed to create policy: ' + (err.error?.message || err.message);
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
    this.claimForm = { policyId: '', description: '', claimAmount: 0 };
    this.claimMessage = '';
    this.selectedFile = null;
    this.showClaimModal = true;
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  submitClaim() {
    if (!this.claimForm.policyId || !this.claimForm.description || !this.claimForm.claimAmount) {
      this.claimMessage = 'Please fill all fields.';
      return;
    }
    this.claimService.submitClaim(this.claimForm).subscribe({
      next: (res: any) => {
        const cId = res.claimId || res.ClaimId;
        // Upload document if selected
        if (this.selectedFile) {
          const formData = new FormData();
          formData.append('file', this.selectedFile);
          this.claimService.uploadDocument(cId, formData).subscribe({
            next: () => {
               this.claimService.submitClaimToReview(cId).subscribe(() => {
                 this.claimMessage = 'Success: Claim and document submitted to review.';
                 this.fetchData();
                 setTimeout(() => this.showClaimModal = false, 1500);
               });
            },
            error: () => this.claimMessage = 'Claim created, but document upload failed.'
          });
        } else {
           this.claimService.submitClaimToReview(cId).subscribe(() => {
             this.claimMessage = 'Success: Claim submitted to review.';
             this.fetchData();
             setTimeout(() => this.showClaimModal = false, 1500);
           });
        }
      },
      error: (err) => this.claimMessage = 'Failed to submit claim: ' + (err.error?.message || err.message)
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
}
