import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-insurance-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card-container">
      <div class="card">
        <!-- Icon Section -->
        <div class="icon-section">
          <div class="icon-wrapper" [class]="'icon-' + getInsuranceType()">
            <ng-container [ngSwitch]="getInsuranceType()">
              <!-- Vehicle Icon -->
              <svg *ngSwitchCase="'vehicle'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="1" y="3" width="15" height="13"></rect>
                <polygon points="16 8 20 8 23 11 23 16 16 16 16 8"></polygon>
                <circle cx="5.5" cy="18.5" r="2.5"></circle>
                <circle cx="18.5" cy="18.5" r="2.5"></circle>
              </svg>
              <!-- Home Icon -->
              <svg *ngSwitchCase="'home'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
                <polyline points="9 22 9 12 15 12 15 22"></polyline>
              </svg>
              <!-- Health Icon -->
              <svg *ngSwitchCase="'health'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
              </svg>
              <!-- Default/Travel Icon -->
              <svg *ngSwitchDefault viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polygon points="12 2 19 21 12 17 5 21 12 2"></polygon>
              </svg>
            </ng-container>
          </div>
        </div>

        <!-- Content Section -->
        <div class="content-section">
          <h3 class="card-title">{{ title }}</h3>
          <p class="card-description">{{ description }}</p>
          
          <!-- Features List -->
          <div *ngIf="features && features.length > 0" class="features-list">
            <div *ngFor="let feature of features" class="feature-item">
              <span class="feature-check">✓</span>
              <span>{{ feature }}</span>
            </div>
          </div>
        </div>

        <!-- Footer Section -->
        <div class="footer-section">
          <div class="badge">Active Plan</div>
          <button 
            class="btn-purchase" 
            (click)="onPurchase.emit(subTypeId)"
            [disabled]="loading">
            {{ loading ? 'Loading...' : 'Get Plan' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card-container {
      perspective: 1000px;
      height: 100%;
    }

    .card {
      background: white;
      border-radius: 0;
      padding: 40px 30px;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
      border-top: 4px solid #0DB18C;
      display: flex;
      flex-direction: column;
      height: 100%;
      transition: transform 0.3s;
      position: relative;
    }

    .card:hover {
      transform: translateY(-5px);
    }

    .icon-section {
      margin-bottom: 30px;
      display: flex;
      justify-content: flex-start;
    }

    .icon-wrapper {
      width: 60px;
      height: 60px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #0DB18C;
      font-size: 30px;
      background: #E6FAF5;
      border-radius: 12px;
      transition: all 0.3s;
    }

    .icon-wrapper svg {
      width: 32px;
      height: 32px;
      stroke-width: 2;
    }

    .card:hover .icon-wrapper {
      background: #0DB18C;
      color: white;
      transform: rotate(-10deg);
    }

    .content-section {
      flex: 1;
      margin-bottom: 24px;
      text-align: left;
    }

    .card-title {
      font-size: 1.6rem;
      font-weight: 700;
      color: #1A1A1A;
      margin-bottom: 15px;
      line-height: 1.2;
    }

    .card-description {
      font-size: 1.05rem;
      color: #666666;
      line-height: 1.6;
      margin-bottom: 25px;
    }

    .features-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .feature-item {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 1rem;
      color: #444444;
    }

    .feature-check {
      color: #0DB18C;
      font-weight: 800;
      flex-shrink: 0;
    }

    .footer-section {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-top: 24px;
      border-top: 1px solid #E5E5E5;
      gap: 12px;
    }

    .badge {
      display: inline-block;
      padding: 6px 12px;
      background: #E6FAF5;
      color: #0DB18C;
      font-size: 0.85rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .btn-purchase {
      flex: 1;
      padding: 12px 20px;
      background: #0DB18C;
      color: white;
      border: none;
      border-radius: 4px;
      font-weight: 700;
      font-size: 0.95rem;
      cursor: pointer;
      transition: all 0.3s;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .btn-purchase:hover:not(:disabled) {
      background: #0A8E70;
      transform: translateY(-2px);
    }

    .btn-purchase:active:not(:disabled) {
      transform: translateY(0);
    }

    .btn-purchase:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .card {
        padding: 24px;
      }
      .card-title {
        font-size: 1.4rem;
      }
    }
  `]
})
export class InsuranceCardComponent {
  @Input() title: string = '';
  @Input() description: string = '';
  @Input() features: string[] = [];
  @Input() subTypeId: string = '';
  @Input() loading: boolean = false;
  @Input() insuranceTypeName: string = '';
  @Output() onPurchase = new EventEmitter<string>();

  ngOnChanges() {
    console.log('InsuranceCardComponent inputs:', {
      title: this.title,
      description: this.description,
      features: this.features,
      subTypeId: this.subTypeId,
      insuranceTypeName: this.insuranceTypeName
    });
  }

  getInsuranceType(): string {
    const type = this.insuranceTypeName?.toLowerCase() || this.title?.toLowerCase() || '';
    if (type.includes('vehicle') || type.includes('auto') || type.includes('car')) return 'vehicle';
    if (type.includes('home') || type.includes('property') || type.includes('house')) return 'home';
    if (type.includes('health') || type.includes('medical')) return 'health';
    if (type.includes('travel')) return 'travel';
    return 'travel';
  }
}
