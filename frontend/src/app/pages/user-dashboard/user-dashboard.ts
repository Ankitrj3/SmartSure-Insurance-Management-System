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
  homeDetails: any = { address: '', propertyType: 'House', estimatedValue: 0 };
  vehicleDetails: any = { registrationNumber: '', make: '', model: '', estimatedValue: 0, chassisNumber: '', engineNumber: '' };
  policyMessage = '';
  calculatedPremium = 0;
  paymentProcessing = false;
  selectedPlanName = '';
  paymentMethod = 'CreditCard';

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
        this.policies = Array.isArray(result.policies) ? result.policies : [];
        this.claims = Array.isArray(result.claims) ? result.claims : [];
        this.insuranceTypes = (Array.isArray(result.types) ? result.types : []).filter((t: any) => 
          t.name?.toLowerCase().includes('vehicle') || t.name?.toLowerCase().includes('home')
        );
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error(err);
        this.loading = false;
      }
    });
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
    this.showBuyPolicyModal = true;
  }

  loadSubtypes() {
    if (!this.selectedType) return;
    this.policyService.getSubtypesByType(this.selectedType).subscribe({
      next: (data) => {
        this.subtypes = data;
        this.updateCalculatedPremium();
      }
    });
  }

  updateCalculatedPremium() {
    const subtype = this.subtypes.find(s => s.subtypeId === this.policyForm.subtypeId);
    if (subtype) {
      this.selectedPlanName = subtype.name;
      this.calculatedPremium = (subtype.basePremium || 0) * (this.policyForm.duration / 12);
      // Ensure at least base premium if duration is small
      if (this.calculatedPremium < (subtype.basePremium || 0)) {
        this.calculatedPremium = subtype.basePremium || 0;
      }
    }
  }

  proceedToReview() {
    const typeObj = this.insuranceTypes.find(t => (t.id || t.typeId) === this.selectedType);
    const isVehicle = typeObj?.name?.toLowerCase().includes('vehicle');
    const isHome = typeObj?.name?.toLowerCase().includes('home');

    if (isHome && (!this.homeDetails.address || !this.homeDetails.propertyType || !this.homeDetails.estimatedValue)) {
      this.policyMessage = 'Please fill all required home details.';
      return;
    }
    if (isVehicle && (!this.vehicleDetails.registrationNumber || !this.vehicleDetails.make || !this.vehicleDetails.model || !this.vehicleDetails.estimatedValue)) {
      this.policyMessage = 'Please fill all required vehicle details.';
      return;
    }

    this.policyMessage = '';
    this.buyPolicyStep = 3;
    this.updateCalculatedPremium();
  }

  proceedToPayment() {
    this.buyPolicyStep = 4;
  }

  submitPolicy() {
    this.paymentProcessing = true;
    this.policyMessage = 'Processing secure payment and activating policy...';

    const typeObj = this.insuranceTypes.find(t => (t.id || t.typeId) === this.selectedType);
    const isVehicle = typeObj?.name?.toLowerCase().includes('vehicle');
    const isHome = typeObj?.name?.toLowerCase().includes('home');

    const payload: any = {
      subtypeId: this.policyForm.subtypeId,
      duration: this.policyForm.duration,
    };

    if (isHome) {
      payload.homeDetail = {
        address: this.homeDetails.address,
        propertyType: this.homeDetails.propertyType || 'Residential',
        yearBuilt: this.homeDetails.yearBuilt || 2024,
        estimatedValue: this.homeDetails.estimatedValue,
        securityFeatures: this.homeDetails.securityFeatures || 'Standard'
      };
    }

    if (isVehicle) {
      payload.vehicleDetail = {
        registrationNumber: this.vehicleDetails.registrationNumber,
        make: this.vehicleDetails.make,
        model: this.vehicleDetails.model,
        manufactureYear: this.vehicleDetails.manufactureYear || 2024,
        estimatedValue: this.vehicleDetails.estimatedValue,
        chassisNumber: this.vehicleDetails.chassisNumber || 'CH' + Math.random().toString(36).substring(7).toUpperCase(),
        engineNumber: this.vehicleDetails.engineNumber || 'EN' + Math.random().toString(36).substring(7).toUpperCase()
      };
    }

    this.policyService.createHomePolicy(payload).subscribe({
      next: (res) => {
        // Now process the payment in the background
        this.policyService.processPayment(res.policyId, {
          policyId: res.policyId,
          amount: res.premiumAmount,
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
    this.paymentPolicyId = policy.policyId;
    this.paymentForm.amount = policy.premiumAmount || 0;
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
        this.paymentMessage = 'Success: Payment processed.';
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
      next: (res) => {
        // Upload document if selected
        if (this.selectedFile) {
          const formData = new FormData();
          formData.append('file', this.selectedFile);
          this.claimService.uploadDocument(res.claimId, formData).subscribe({
            next: () => {
               this.claimService.submitClaimToReview(res.claimId).subscribe(() => {
                 this.claimMessage = 'Success: Claim and document submitted to review.';
                 this.fetchData();
                 setTimeout(() => this.showClaimModal = false, 1500);
               });
            },
            error: () => this.claimMessage = 'Claim created, but document upload failed.'
          });
        } else {
           this.claimService.submitClaimToReview(res.claimId).subscribe(() => {
             this.claimMessage = 'Success: Claim submitted to review.';
             this.fetchData();
             setTimeout(() => this.showClaimModal = false, 1500);
           });
        }
      },
      error: (err) => this.claimMessage = 'Failed to submit claim: ' + (err.error?.message || err.message)
    });
  }
}
