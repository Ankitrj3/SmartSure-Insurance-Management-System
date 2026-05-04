import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PolicyService } from '../../core/services/policy.service';
import { AuthService } from '../../core/services/auth.service';
import { InsuranceCardComponent } from '../../components/insurance-card/insurance-card';

const INITIAL_VISIBLE = 6;

@Component({
  selector: 'app-insurance',
  standalone: true,
  imports: [CommonModule, InsuranceCardComponent],
  template: `
    <div class="page-container">
      <!-- Header Banner -->
      <div class="header-banner">
        <div class="header-content">
          <span class="badge-label">OUR PLANS</span>
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
          <div class="no-data-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
          </div>
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
                <div class="category-title-row">
                  <div class="category-accent-bar"></div>
                  <h2 class="category-title">{{ type.name || type.Name }}</h2>
                </div>
                <p class="category-desc">{{ type.description || type.Description || 'Premium coverage plans for comprehensive protection.' }}</p>
              </div>

              <!-- Plans Grid for this Category -->
              <div class="plans-grid">
                @for (subtype of getVisibleSubtypes(type); track subtype.subtypeId || subtype.SubtypeId) {
                  <app-insurance-card
                    [title]="subtype.name || subtype.Name"
                    [description]="subtype.description || subtype.Description"
                    [subTypeId]="subtype.insuranceSubTypeId || subtype.InsuranceSubTypeId || subtype.id || subtype.Id"
                    [insuranceTypeName]="type.name || type.Name"
                    [features]="getFeatures(subtype)"
                    [basePremium]="subtype.basePremium || subtype.BasePremium"
                    [loading]="false"
                    (onPurchase)="openPolicyDetails($event, type, subtype)">
                  </app-insurance-card>
                }
              </div>

              <!-- Show More / Show Less Button -->
              @if (hasMoreSubtypes(type)) {
                <div class="show-more-wrapper">
                  <button class="btn-show-more" (click)="toggleShowMore(type)">
                    @if (isExpanded(type)) {
                      <span>Show Less</span>
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                        <polyline points="18 15 12 9 6 15"></polyline>
                      </svg>
                    } @else {
                      <span>Show More ({{ getRemainingCount(type) }} more plans)</span>
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                        <polyline points="6 9 12 15 18 9"></polyline>
                      </svg>
                    }
                  </button>
                </div>
              }

              <!-- Empty State for Category -->
              @if (getSubtypesForType(type).length === 0) {
                <div class="empty-category">
                  <div class="empty-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <line x1="12" y1="5" x2="12" y2="19"></line>
                      <line x1="5" y1="12" x2="19" y2="12"></line>
                    </svg>
                  </div>
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
          <div class="error-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
          </div>
          <h2>Something went wrong</h2>
          <p>{{ error() }}</p>
          <button class="btn-retry" (click)="fetchPublicPlans()">Try Again</button>
        </div>
      }

      <!-- Policy Details Modal -->
      @if (selectedSubtype()) {
        <div class="modal-overlay" (click)="closeModal()">
          <div class="modal-content" (click)="$event.stopPropagation()">
            <div class="modal-header">
              <div class="modal-title-group">
                <span class="modal-badge">{{ selectedType()?.name || selectedType()?.Name }}</span>
                <h2>{{ selectedSubtype()?.name || selectedSubtype()?.Name }}</h2>
              </div>
              <button class="btn-close" (click)="closeModal()">×</button>
            </div>
            <div class="modal-body">
              <p class="modal-desc">{{ selectedSubtype()?.description || selectedSubtype()?.Description }}</p>
              
              <div class="modal-features">
                <h3>Plan Features & Benefits</h3>
                <ul>
                  @for (feature of getFeatures(selectedSubtype()); track feature) {
                    <li>
                      <span class="feature-check">✓</span>
                      {{ feature }}
                    </li>
                  }
                </ul>
              </div>
              
              <div class="modal-details-grid">
                <div class="detail-item">
                  <span class="detail-label">Coverage Overview</span>
                  <span class="detail-value">Comprehensive</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Customer Support</span>
                  <span class="detail-value">24/7 Priority</span>
                </div>
                @if (selectedSubtype()?.basePremium || selectedSubtype()?.BasePremium) {
                  <div class="detail-item">
                    <span class="detail-label">Starting Premium</span>
                    <span class="detail-value" style="color:#0DB18C; font-size:1.4rem;">₹{{ (selectedSubtype()?.basePremium || selectedSubtype()?.BasePremium) | number:'1.0-0' }}<small style="font-size:0.75rem;color:#888;font-weight:600;">/yr</small></span>
                  </div>
                }
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn-outline" (click)="closeModal()">Cancel</button>
              <button class="btn-proceed" (click)="proceedToBuy()">Proceed to Buy</button>
            </div>
          </div>
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
      background: #1A1A1A;
      color: #FFFFFF;
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

    .badge-label {
      display: inline-block;
      background-color: #F7F072;
      color: #1A1A1A;
      padding: 6px 16px;
      font-weight: 800;
      font-size: 0.8rem;
      letter-spacing: 2px;
      margin-bottom: 24px;
      text-transform: uppercase;
    }

    .header-title {
      font-size: 3.5rem;
      font-weight: 800;
      margin-bottom: 20px;
      color: #FFFFFF;
      letter-spacing: -0.02em;
    }

    .header-subtitle {
      font-size: 1.25rem;
      font-weight: 600;
      color: #0DB18C;
      margin-bottom: 12px;
    }

    .header-description {
      font-size: 1.05rem;
      color: #AAAAAA;
      line-height: 1.6;
    }

    /* Plans Section */
    .plans-section {
      padding: 80px 5%;
      max-width: 1400px;
      margin: 0 auto;
    }

    .insurance-category {
      margin-bottom: 90px;
    }

    .category-header {
      margin-bottom: 48px;
      text-align: left;
    }

    .category-title-row {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 16px;
    }

    .category-accent-bar {
      width: 6px;
      height: 48px;
      background-color: #0DB18C;
      flex-shrink: 0;
    }

    .category-title {
      font-size: 2.5rem;
      font-weight: 800;
      color: #1A1A1A;
      letter-spacing: -0.02em;
    }

    .category-desc {
      font-size: 1.1rem;
      color: #666666;
      max-width: 700px;
      line-height: 1.65;
      padding-left: 22px;
    }

    /* Plans Grid */
    .plans-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 28px;
      margin-bottom: 32px;
    }

    /* Show More Button */
    .show-more-wrapper {
      display: flex;
      justify-content: center;
      padding: 8px 0 20px;
    }

    .btn-show-more {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      background: #FFFFFF;
      border: 2px solid #0DB18C;
      color: #0DB18C;
      padding: 14px 32px;
      font-weight: 700;
      font-size: 0.95rem;
      cursor: pointer;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      transition: all 0.25s ease;
    }

    .btn-show-more:hover {
      background: #0DB18C;
      color: #FFFFFF;
    }

    .btn-show-more svg {
      width: 18px;
      height: 18px;
      transition: transform 0.25s ease;
    }

    /* Empty category */
    .empty-category {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 60px;
      text-align: center;
      background: #F8FAFC;
      border: 2px dashed #0DB18C;
      color: #666666;
    }

    .empty-icon {
      width: 64px;
      height: 64px;
      margin-bottom: 20px;
      color: #0DB18C;
    }

    .empty-icon svg {
      width: 100%;
      height: 100%;
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
      width: 52px;
      height: 52px;
      border: 4px solid #E5E5E5;
      border-top-color: #0DB18C;
      border-radius: 50%;
      animation: spin 0.9s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .loader-container p {
      font-size: 1.05rem;
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
      width: 80px;
      height: 80px;
      margin: 0 auto 24px;
      color: #0DB18C;
    }

    .no-data-icon svg {
      width: 100%;
      height: 100%;
    }

    .no-data-state h2 {
      font-size: 2rem;
      color: #1A1A1A;
      margin-bottom: 12px;
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
    }

    .error-icon {
      width: 80px;
      height: 80px;
      margin: 0 auto 24px;
      color: #DC2626;
    }

    .error-icon svg {
      width: 100%;
      height: 100%;
    }

    .error-state h2 {
      font-size: 2rem;
      color: #1A1A1A;
      margin-bottom: 12px;
      font-weight: 800;
    }

    .error-state p {
      font-size: 1.1rem;
      max-width: 500px;
      margin: 0 auto 28px;
      line-height: 1.6;
      color: #666;
    }

    .btn-retry {
      padding: 14px 36px;
      background: #0DB18C;
      color: white;
      border: none;
      font-weight: 700;
      font-size: 1rem;
      cursor: pointer;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      transition: background 0.25s;
    }

    .btn-retry:hover {
      background: #0A8E70;
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
        padding: 70px 5%;
      }
      .header-title {
        font-size: 2.5rem;
      }
      .plans-section {
        padding: 50px 5%;
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
      .category-title {
        font-size: 1.6rem;
      }
    }

    /* Modal Styles */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background: rgba(0, 0, 0, 0.7);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
      backdrop-filter: blur(4px);
    }

    .modal-content {
      background: #FFFFFF;
      width: 90%;
      max-width: 650px;
      padding: 0;
      border: 1px solid #1A1A1A;
      position: relative;
      animation: modalSlide 0.3s ease-out;
      border-radius: 0;
    }

    @keyframes modalSlide {
      from { transform: translateY(20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }

    .modal-header {
      padding: 30px 40px 20px;
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      background: #FDFDE6;
      border-bottom: 4px solid #0DB18C;
    }

    .modal-title-group .modal-badge {
      display: inline-block;
      background: #1A1A1A;
      color: #F7F072;
      padding: 4px 12px;
      font-size: 0.8rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 1px;
      margin-bottom: 12px;
    }

    .modal-title-group h2 {
      font-size: 2rem;
      font-weight: 800;
      color: #1A1A1A;
      margin: 0;
      letter-spacing: -0.02em;
    }

    .btn-close {
      background: transparent;
      border: none;
      font-size: 2.5rem;
      color: #1A1A1A;
      cursor: pointer;
      line-height: 0.8;
      padding: 0;
      margin-top: -5px;
      transition: color 0.2s;
    }

    .btn-close:hover {
      color: #DC2626;
    }

    .modal-body {
      padding: 30px 40px;
    }

    .modal-desc {
      font-size: 1.1rem;
      color: #666666;
      line-height: 1.6;
      margin-bottom: 30px;
    }

    .modal-features {
      margin-bottom: 30px;
    }

    .modal-features h3 {
      font-size: 1.2rem;
      font-weight: 800;
      color: #1A1A1A;
      margin-bottom: 16px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      border-left: 4px solid #0DB18C;
      padding-left: 12px;
    }

    .modal-features ul {
      list-style: none;
      padding: 0;
      margin: 0;
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .modal-features li {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 1rem;
      color: #444444;
      font-weight: 600;
    }

    .modal-features .feature-check {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 24px;
      height: 24px;
      background: #0DB18C;
      color: white;
      font-weight: 800;
      font-size: 0.9rem;
    }

    .modal-details-grid {
      display: flex;
      gap: 20px;
      background: #F8FAFC;
      padding: 20px;
      border: 1px dashed #CCCCCC;
    }

    .detail-item {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .detail-label {
      font-size: 0.85rem;
      color: #666666;
      text-transform: uppercase;
      font-weight: 700;
      letter-spacing: 0.5px;
    }

    .detail-value {
      font-size: 1.1rem;
      color: #1A1A1A;
      font-weight: 800;
    }

    .modal-footer {
      padding: 24px 40px;
      border-top: 1px solid #EEEEEE;
      display: flex;
      justify-content: flex-end;
      gap: 16px;
      background: #FAFAFA;
    }

    .btn-outline {
      padding: 12px 24px;
      background: transparent;
      border: 2px solid #1A1A1A;
      color: #1A1A1A;
      font-weight: 700;
      cursor: pointer;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      transition: all 0.2s;
    }

    .btn-outline:hover {
      background: #1A1A1A;
      color: #FFFFFF;
    }

    .btn-proceed {
      padding: 12px 32px;
      background: #0DB18C;
      border: none;
      color: #FFFFFF;
      font-weight: 700;
      cursor: pointer;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      transition: all 0.25s;
    }

    .btn-proceed:hover {
      background: #0A8E70;
      transform: translateY(-2px);
    }
    
    @media (max-width: 768px) {
      .modal-features ul {
        grid-template-columns: 1fr;
      }
      .modal-header, .modal-body, .modal-footer {
        padding: 20px;
      }
      .modal-title-group h2 {
        font-size: 1.6rem;
      }
      .modal-details-grid {
        flex-direction: column;
      }
    }
  `]
})
export class Insurance implements OnInit {
  insuranceTypes = signal<any[]>([]);
  insuranceSubtypes = signal<any[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  // Track expanded state per insurance type
  private expandedTypes = signal<Set<string>>(new Set());

  // Track selected plan for modal
  selectedType = signal<any>(null);
  selectedSubtype = signal<any>(null);

  constructor(
    private policyService: PolicyService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.fetchPublicPlans();
  }

  fetchPublicPlans() {
    this.loading.set(true);
    this.error.set(null);

    this.policyService.getInsuranceTypes().subscribe({
      next: (response: any) => {
        let types = response;
        if (response?.data) types = response.data;
        if (types?.$values) types = types.$values;

        const typesArray = Array.isArray(types) ? types : [];
        this.insuranceTypes.set(typesArray);

        if (typesArray.length === 0) {
          this.loading.set(false);
          return;
        }

        this.policyService.getInsuranceSubtypes().subscribe({
          next: (response2: any) => {
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
            this.loading.set(false);
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

  getTypeKey(type: any): string {
    return String(type.typeId || type.TypeId || type.id || type.Id || '');
  }

  isExpanded(type: any): boolean {
    return this.expandedTypes().has(this.getTypeKey(type));
  }

  hasMoreSubtypes(type: any): boolean {
    return this.getSubtypesForType(type).length > INITIAL_VISIBLE;
  }

  getRemainingCount(type: any): number {
    const total = this.getSubtypesForType(type).length;
    return Math.max(0, total - INITIAL_VISIBLE);
  }

  getVisibleSubtypes(type: any): any[] {
    const all = this.getSubtypesForType(type);
    if (this.isExpanded(type)) return all;
    return all.slice(0, INITIAL_VISIBLE);
  }

  toggleShowMore(type: any) {
    const key = this.getTypeKey(type);
    const current = new Set(this.expandedTypes());
    if (current.has(key)) {
      current.delete(key);
    } else {
      current.add(key);
    }
    this.expandedTypes.set(current);
  }

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

  openPolicyDetails(subtypeId: string, type: any, subtype: any) {
    this.selectedType.set(type);
    this.selectedSubtype.set(subtype);
    document.body.style.overflow = 'hidden';
  }

  closeModal() {
    this.selectedType.set(null);
    this.selectedSubtype.set(null);
    document.body.style.overflow = 'auto';
  }

  proceedToBuy() {
    const type = this.selectedType();
    const subtype = this.selectedSubtype();
    
    if (!type || !subtype) return;
    
    const subtypeId = subtype.insuranceSubTypeId || subtype.InsuranceSubTypeId || subtype.id || subtype.Id;
    const typeId = type.typeId || type.TypeId || type.id || type.Id;

    this.closeModal();

    if (this.authService.hasToken()) {
      this.router.navigate(['/user-dashboard'], {
        queryParams: {
          typeId: typeId,
          subtypeId: subtypeId,
          action: 'buy'
        }
      });
    } else {
      this.router.navigate(['/login']);
    }
  }
}
