import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PolicyService } from '../../core/services/policy.service';
import { AuthService } from '../../core/services/auth.service';
import { InsuranceCardComponent } from '../../components/insurance-card/insurance-card';

@Component({
  selector: 'app-insurance',
  standalone: true,
  imports: [CommonModule, InsuranceCardComponent],
  template: `
    <div class="page-container">
      <!-- Header Banner -->
      <div class="header-banner">
        <div class="header-content">
          <span class="badge-yellow">OUR PLANS</span>
          <h1 class="header-title">Our Insurance Portfolio</h1>
          <p class="header-subtitle">Premium coverage for Vehicle, Home, Health, and more.</p>
          <p class="header-description">Comprehensive protection engineered for maximum security and peace of mind.</p>
        </div>
      </div>

      <!-- Loading State -->
      @if (loading()) {
        <div class="loader-container">
          <div class="spinner"></div>
          <p>Loading insurance plans...</p>
        </div>
      }

      <!-- No Data State -->
      @if (!loading() && insuranceTypes().length === 0) {
        <div class="no-data-state">
          <div class="no-data-icon">i</div>
          <h2>No Plans Available</h2>
          <p>Our team is currently updating our insurance offerings. Please check back soon!</p>
        </div>
      }

      <!-- Insurance Plans Grid -->
      @if (!loading() && insuranceTypes().length > 0) {
        <div class="plans-section">
          @for (type of insuranceTypes(); track type.typeId || type.TypeId) {
            <div class="insurance-category">
              <div class="category-header">
                <h2 class="category-title">{{ type.name || type.Name }}</h2>
                <p class="category-desc">{{ type.description || type.Description || 'Premium coverage plans for comprehensive protection.' }}</p>
              </div>

              <!-- Plans Grid for this Category -->
              <div class="plans-grid">
                @for (subtype of getSubtypesForType(type); track subtype.subtypeId || subtype.SubtypeId) {
                  <app-insurance-card
                    [title]="subtype.name || subtype.Name"
                    [description]="subtype.description || subtype.Description"
                    [subTypeId]="subtype.insuranceSubTypeId || subtype.InsuranceSubTypeId || subtype.id || subtype.Id"
                    [insuranceTypeName]="type.name || type.Name"
                    [features]="getFeatures(subtype)"
                    [loading]="false"
                    (onPurchase)="handlePurchase($event, type, subtype)">
                  </app-insurance-card>
                }
              </div>

              <!-- Empty State for Category -->
              @if (getSubtypesForType(type).length === 0) {
                <div class="empty-category">
                  <div class="empty-icon">+</div>
                  <p>New plans for {{ type.name || type.Name }} coming soon!</p>
                </div>
              }
            </div>
          }
        </div>
      }

      <!-- Error State -->
      @if (!loading() && error()) {
        <div class="error-state">
          <div class="error-icon">!</div>
          <h2>Something went wrong</h2>
          <p>{{ error() }}</p>
          <button class="btn-retry" (click)="fetchPublicPlans()">Retry</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container {
      background: #FFFFFF;
      min-height: 100vh;
      padding: 0;
      overflow-x: hidden;
      font-family: 'Inter', sans-serif;
    }

    /* Header Banner */
    .header-banner {
      background: #F8FAFC;
      color: #1A1A1A;
      padding: 100px 5%;
      text-align: center;
      position: relative;
    }

    .header-content {
      position: relative;
      z-index: 1;
      max-width: 800px;
      margin: 0 auto;
    }

    .badge-yellow {
      display: inline-block;
      background-color: #F7F072;
      color: #1A1A1A;
      padding: 6px 16px;
      font-weight: 800;
      font-size: 0.85rem;
      letter-spacing: 1px;
      margin-bottom: 20px;
      text-transform: uppercase;
    }

    .header-title {
      font-size: 3.5rem;
      font-weight: 800;
      margin-bottom: 20px;
      color: #1A1A1A;
    }

    .header-subtitle {
      font-size: 1.5rem;
      font-weight: 700;
      color: #0DB18C;
      margin-bottom: 12px;
    }

    .header-description {
      font-size: 1.1rem;
      color: #666666;
      line-height: 1.6;
    }

    /* Plans Section */
    .plans-section {
      padding: 80px 5%;
      max-width: 1400px;
      margin: 0 auto;
    }

    .insurance-category {
      margin-bottom: 80px;
    }

    .category-header {
      margin-bottom: 48px;
      text-align: left;
    }

    .category-title {
      font-size: 2.5rem;
      font-weight: 800;
      color: #1A1A1A;
      margin-bottom: 12px;
      padding-bottom: 15px;
      border-bottom: 4px solid #0DB18C;
      display: inline-block;
    }

    .category-desc {
      font-size: 1.15rem;
      color: #666666;
      max-width: 800px;
      line-height: 1.6;
      margin-top: 15px;
    }

    /* Plans Grid */
    .plans-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 32px;
      margin-bottom: 40px;
    }

    /* Empty State */
    .empty-category {
      grid-column: 1 / -1;
      padding: 60px;
      text-align: center;
      background: #F8FAFC;
      border: 2px dashed #0DB18C;
      border-radius: 0;
      color: #666666;
    }

    .empty-icon {
      font-size: 3rem;
      margin-bottom: 16px;
    }

    .empty-category p {
      font-size: 1.1rem;
      font-weight: 600;
      color: #1A1A1A;
    }

    /* Loading State */
    .loader-container {
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      min-height: 400px;
      gap: 20px;
    }

    .spinner {
      width: 56px;
      height: 56px;
      border: 4px solid #E6FAF5;
      border-top-color: #0DB18C;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .loader-container p {
      font-size: 1.1rem;
      color: #666666;
      font-weight: 500;
    }

    /* No Data State */
    .no-data-state {
      text-align: center;
      padding: 100px 20px;
      color: #666666;
    }

    .no-data-icon {
      font-size: 4rem;
      margin-bottom: 20px;
    }

    .no-data-state h2 {
      font-size: 2rem;
      color: #1A1A1A;
      margin-bottom: 10px;
      font-weight: 800;
    }

    .no-data-state p {
      font-size: 1.1rem;
      max-width: 500px;
      margin: 0 auto;
      line-height: 1.6;
    }

    /* Error State */
    .error-state {
      text-align: center;
      padding: 100px 20px;
      color: #DC2626;
    }

    .error-icon {
      font-size: 4rem;
      margin-bottom: 20px;
    }

    .error-state h2 {
      font-size: 2rem;
      color: #991B1B;
      margin-bottom: 10px;
      font-weight: 800;
    }

    .error-state p {
      font-size: 1.1rem;
      max-width: 500px;
      margin: 20px auto;
      line-height: 1.6;
    }

    .btn-retry {
      margin-top: 20px;
      padding: 15px 32px;
      background: #0DB18C;
      color: white;
      border: none;
      border-radius: 0;
      font-weight: 700;
      font-size: 1rem;
      cursor: pointer;
      text-transform: uppercase;
      transition: all 0.3s;
    }

    .btn-retry:hover {
      background: #0A8E70;
      transform: translateY(-2px);
    }

    /* Responsive */
    @media (max-width: 1024px) {
      .plans-grid {
        grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
        gap: 24px;
      }
    }

    @media (max-width: 768px) {
      .header-banner {
        padding: 60px 5%;
      }

      .header-title {
        font-size: 2.5rem;
      }

      .header-subtitle {
        font-size: 1.2rem;
      }

      .plans-section {
        padding: 40px 5%;
      }

      .category-title {
        font-size: 2rem;
      }

      .plans-grid {
        grid-template-columns: 1fr;
        gap: 20px;
      }
    }

    @media (max-width: 480px) {
      .header-title {
        font-size: 2rem;
      }

      .header-subtitle {
        font-size: 1rem;
      }

      .category-title {
        font-size: 1.5rem;
      }

      .plans-grid {
        gap: 16px;
      }
    }
  `]
})
export class Insurance implements OnInit {
  insuranceTypes = signal<any[]>([]);
  insuranceSubtypes = signal<any[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private policyService: PolicyService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    console.log('INSURANCE PAGE INITIALIZED - SIGNAL VERSION');
    this.fetchPublicPlans();
  }

  fetchPublicPlans() {
    this.loading.set(true);
    this.error.set(null);

    this.policyService.getInsuranceTypes().subscribe({
      next: (response: any) => {
        console.log('Types Response:', response);
        
        let types = response;
        if (response?.data) types = response.data;
        if (types?.$values) types = types.$values;
        
        const typesArray = Array.isArray(types) ? types : [];
        this.insuranceTypes.set(typesArray);
        console.log('Types set:', typesArray.length);

        if (typesArray.length === 0) {
          this.loading.set(false);
          return;
        }

        this.policyService.getInsuranceSubtypes().subscribe({
          next: (response2: any) => {
            console.log('Subtypes Response:', response2);
            
            let subtypes = response2;
            if (response2?.data) subtypes = response2.data;
            if (subtypes?.$values) subtypes = subtypes.$values;
            
            const allSubtypes = Array.isArray(subtypes) ? subtypes : [];

            const mappedSubtypes = allSubtypes.map(s => {
              const sTypeId = s.typeId || s.TypeId || s.insuranceTypeId || s.InsuranceTypeId;
              const parent = typesArray.find(t => {
                const tId = t.typeId || t.TypeId || t.id || t.Id;
                return tId === sTypeId;
              });
              return { 
                ...s, 
                typeName: parent?.name || parent?.Name || 'Unknown' 
              };
            });

            this.insuranceSubtypes.set(mappedSubtypes);
            console.log('Subtypes set:', mappedSubtypes.length);
            this.loading.set(false);
            console.log('Loading complete!');
          },
          error: (err) => {
            console.error('Subtypes error:', err);
            this.error.set('Failed to load insurance subtypes. Please try again.');
            this.loading.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Types error:', err);
        this.error.set('Failed to load insurance types. Please try again.');
        this.loading.set(false);
      }
    });
  }

  getSubtypesForType(type: any): any[] {
    const typeId = type.typeId || type.TypeId || type.id || type.Id;
    
    if (!typeId) return [];
    
    return this.insuranceSubtypes().filter(s => {
      const sTypeId = s.typeId || s.TypeId || s.insuranceTypeId || s.InsuranceTypeId;
      return sTypeId === typeId;
    });
  }

  /**
   * Generate features based on subtype
   */
  getFeatures(subtype: any): string[] {
    const name = (subtype.name || subtype.Name || '').toLowerCase();
    
    if (name.includes('comprehensive')) {
      return ['Full coverage', 'Zero deductible', '24/7 support', 'Emergency service'];
    }
    if (name.includes('third party')) {
      return ['Affordable rates', 'Legal protection', 'Roadside assistance'];
    }
    if (name.includes('home')) {
      return ['Building coverage', 'Contents included', 'Personal liability', 'Accidental damage'];
    }
    if (name.includes('fire')) {
      return ['Fire damage', 'Theft protection', 'Natural disaster coverage'];
    }
    
    return ['Premium coverage', 'Expert support', 'Quick settlement', 'Hassle-free claims'];
  }

  handlePurchase(subtypeId: string, type: any, subtype: any) {
    console.log('Purchase clicked:', { subtypeId, type, subtype });
    
    if (this.authService.hasToken()) {
      this.router.navigate(['/user-dashboard/buy-policy'], {
        queryParams: {
          typeId: type.typeId || type.TypeId || type.id || type.Id,
          subtypeId: subtypeId
        }
      });
    } else {
      this.router.navigate(['/login']);
    }
  }
}
