import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PolicyService } from '../../core/services/policy.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-insurance',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-container">
      <div class="header-banner">
        <h1>Our Insurance Portfolio</h1>
        <p>Premium coverage for Vehicle, Home, and more.</p>
      </div>

      <div *ngIf="loading" class="loader-container">
        <div class="spinner"></div>
      </div>

      <div *ngIf="!loading && insuranceTypes.length === 0" class="no-data-state">
         <p>No insurance categories found at this moment. Our team is updating our offerings.</p>
      </div>

      <div *ngIf="!loading" class="offerings-container">
        <div class="category-block" *ngFor="let type of insuranceTypes">
          <div class="category-header">
            <h3>{{ type.name || type.Name }}</h3>
            <p>{{ (type.description || type.Description) || 'Premium coverage plans engineered for comprehensive protection.' }}</p>
          </div>

          <div class="plans-grid">
            <div class="plan-card" *ngFor="let s of getSubtypesForType(type)">
              <div class="plan-icon">
                <svg *ngIf="(type.name || type.Name)?.toLowerCase()?.includes('vehicle') || (type.name || type.Name)?.toLowerCase()?.includes('auto')" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="1" y="3" width="15" height="13"></rect><polygon points="16 8 20 8 23 11 23 16 16 16 16 8"></polygon><circle cx="5.5" cy="18.5" r="2.5"></circle><circle cx="18.5" cy="18.5" r="2.5"></circle></svg>
                <svg *ngIf="(type.name || type.Name)?.toLowerCase()?.includes('home') || (type.name || type.Name)?.toLowerCase()?.includes('property')" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path><polyline points="9 22 9 12 15 12 15 22"></polyline></svg>
                <svg *ngIf="!(type.name || type.Name)?.toLowerCase()?.includes('vehicle') && !(type.name || type.Name)?.toLowerCase()?.includes('auto') && !(type.name || type.Name)?.toLowerCase()?.includes('home') && !(type.name || type.Name)?.toLowerCase()?.includes('property')" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg>
              </div>
              
              <div class="plan-content">
                <h4>{{ s.name || s.Name }}</h4>
                <p>{{ (s.description || s.Description) || 'Dedicated coverage tailored for this specific context, offering maximum value and security.' }}</p>
              </div>

              <div class="plan-footer">
                 <span class="plan-tag">Active Partner</span>
                 <button class="btn btn-purchase" (click)="onPurchaseClick()">Purchase Plan</button>
              </div>
            </div>
            <div *ngIf="getSubtypesForType(type).length === 0" class="empty-state">
               New packages for this category coming soon!
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 60px 5%; background: white; min-height: 80vh; }
    .header-banner { text-align: center; margin-bottom: 60px; }
    .header-banner h1 { font-size: 3rem; color: #10B981; margin-bottom: 15px; }
    .header-banner p { font-size: 1.2rem; color: #64748b; }
    .category-block { max-width: 1280px; margin: 0 auto 60px; }
    .category-header { margin-bottom: 32px; }
    .category-header h3 { font-size: 1.8rem; margin-bottom: 8px; color: #10B981; }
    .plans-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 30px; }
    .plan-card { background: white; border-radius: 16px; padding: 32px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); border: 1px solid #E3E8EE; display: flex; flex-direction: column; transition: transform 0.3s; }
    .plan-card:hover { transform: translateY(-5px); box-shadow: 0 12px 24px rgba(0,0,0,0.1); border-color: #10B981; }
    .plan-icon { width: 56px; height: 56px; background-color: #F0FDF4; color: #10B981; border-radius: 14px; display: flex; align-items: center; justify-content: center; margin-bottom: 24px; }
    .plan-icon svg { width: 28px; height: 28px; }
    .plan-content { flex: 1; }
    .plan-content h4 { font-size: 1.3rem; margin-bottom: 12px; color: #1A1F36; }
    .plan-content p { color: #4F566B; margin-bottom: 24px; }
    .plan-footer { display: flex; align-items: center; justify-content: space-between; border-top: 1px solid #F0F4F8; padding-top: 24px; margin-top: auto; }
    .plan-tag { font-size: 0.8rem; font-weight: 600; color: #10B981; background: #F0FDF4; padding: 4px 10px; border-radius: 4px; }
    .empty-state { grid-column: 1 / -1; padding: 40px; text-align: center; border: 2px dashed #E3E8EE; color: #64748b; border-radius: 16px; min-width: 100%; }
    .no-data-state { text-align: center; padding: 100px 0; color: #64748b; font-size: 1.2rem; }
    .btn { padding: 10px 20px; border-radius: 8px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
    .btn-purchase { background: #10B981; color: white; }
    .btn-purchase:hover { background: #059669; }
    .loader-container { display: flex; justify-content: center; padding: 60px 0; }
    .spinner { width: 48px; height: 48px; border: 4px solid #F0FDF4; border-top-color: #10B981; border-radius: 50%; animation: spin 1s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class Insurance implements OnInit {
  insuranceTypes: any[] = [];
  insuranceSubtypes: any[] = [];
  loading = true;

  constructor(
    private policyService: PolicyService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.fetchPublicPlans();
  }

  fetchPublicPlans() {
    this.policyService.getInsuranceTypes().subscribe({
      next: (typesData: any) => {
        this.insuranceTypes = this.extractData(typesData);
        
        this.policyService.getInsuranceSubtypes().subscribe({
          next: (subtypesData: any) => {
            const allSubtypes = this.extractData(subtypesData);
            this.insuranceSubtypes = allSubtypes.map(s => {
              const sTypeId = s.typeId || s.TypeId || s.insuranceTypeId || s.InsuranceTypeId;
              const parent = this.insuranceTypes.find(t => {
                const tId = t.typeId || t.TypeId || t.id || t.Id;
                return tId === sTypeId;
              });
              return { ...s, typeName: parent?.name || parent?.Name || 'Unknown' };
            });
            this.loading = false;
          },
          error: (err) => {
            console.error('Subtypes fetch err', err);
            this.loading = false;
          }
        });
      },
      error: (err) => {
        console.error('Types fetch err', err);
        this.loading = false;
      }
    });
  }

  private extractData(data: any): any[] {
    if (Array.isArray(data)) return data;
    if (data && data.$values && Array.isArray(data.$values)) return data.$values;
    return [];
  }

  getSubtypesForType(type: any) {
    const typeId = type.typeId || type.TypeId || type.id || type.Id;
    return this.insuranceSubtypes.filter(s => {
       const sTypeId = s.typeId || s.TypeId || s.insuranceTypeId || s.InsuranceTypeId;
       return sTypeId === typeId;
    });
  }

  onPurchaseClick() {
    if (this.authService.hasToken()) {
      this.router.navigate(['/user-dashboard']);
    } else {
      this.router.navigate(['/login']);
    }
  }
}
