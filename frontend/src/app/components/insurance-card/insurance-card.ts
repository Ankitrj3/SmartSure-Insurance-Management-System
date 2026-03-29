import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-insurance-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card-container">
      <div class="card">
        <!-- Top Accent Bar -->
        <div class="card-accent-bar" [class.accent-yellow]="getInsuranceType() === 'home'"></div>

        <!-- Icon Section -->
        <div class="icon-section">
          <div class="icon-wrapper" [class.icon-yellow]="getInsuranceType() === 'home'">
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
                <path d="M9 12l2 2 4-4"></path>
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
          <div class="badge" [class.badge-yellow]="getInsuranceType() === 'home'">Active Plan</div>
          <button
            class="btn-purchase"
            [class.btn-purchase-dark]="getInsuranceType() === 'home'"
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
      height: 100%;
    }

    .card {
      background: white;
      border-radius: 0;
      padding: 36px 28px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
      border: 1px solid #F0F0F0;
      display: flex;
      flex-direction: column;
      height: 100%;
      transition: transform 0.25s ease, box-shadow 0.25s ease;
      position: relative;
    }

    .card:hover {
      transform: translateY(-6px);
      box-shadow: 0 16px 40px rgba(0, 0, 0, 0.10);
    }

    /* Accent top border */
    .card-accent-bar {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 4px;
      background: #0DB18C;
    }

    .card-accent-bar.accent-yellow {
      background: #F7F072;
    }

    /* Icon */
    .icon-section {
      margin-bottom: 28px;
      display: flex;
      justify-content: flex-start;
      padding-top: 8px;
    }

    .icon-wrapper {
      width: 60px;
      height: 60px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #0DB18C;
      background: #E6FAF5;
      border-radius: 0;
      transition: all 0.25s ease;
    }

    .icon-wrapper.icon-yellow {
      color: #1A1A1A;
      background: #FDFDE6;
    }

    .icon-wrapper svg {
      width: 30px;
      height: 30px;
      stroke-width: 2;
    }

    .card:hover .icon-wrapper {
      background: #0DB18C;
      color: white;
    }

    .card:hover .icon-wrapper.icon-yellow {
      background: #1A1A1A;
      color: #F7F072;
    }

    /* Content */
    .content-section {
      flex: 1;
      margin-bottom: 24px;
      text-align: left;
    }

    .card-title {
      font-size: 1.5rem;
      font-weight: 800;
      color: #1A1A1A;
      margin-bottom: 12px;
      line-height: 1.2;
      letter-spacing: -0.02em;
    }

    .card-description {
      font-size: 0.97rem;
      color: #666666;
      line-height: 1.65;
      margin-bottom: 20px;
    }

    .features-list {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .feature-item {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 0.92rem;
      color: #444444;
    }

    .feature-check {
      color: #0DB18C;
      font-weight: 800;
      font-size: 0.88rem;
      flex-shrink: 0;
    }

    /* Footer */
    .footer-section {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-top: 20px;
      border-top: 1px solid #EEEEEE;
      gap: 12px;
    }

    .badge {
      display: inline-block;
      padding: 5px 12px;
      background: #E6FAF5;
      color: #0DB18C;
      font-size: 0.78rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.8px;
    }

    .badge.badge-yellow {
      background: #FDFDE6;
      color: #1A1A1A;
    }

    .btn-purchase {
      flex: 1;
      padding: 12px 18px;
      background: #0DB18C;
      color: white;
      border: none;
      border-radius: 0;
      font-weight: 700;
      font-size: 0.88rem;
      cursor: pointer;
      transition: background 0.2s ease, transform 0.2s ease;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .btn-purchase:hover:not(:disabled) {
      background: #0A8E70;
      transform: translateY(-2px);
    }

    .btn-purchase.btn-purchase-dark {
      background: #1A1A1A;
    }

    .btn-purchase.btn-purchase-dark:hover:not(:disabled) {
      background: #333333;
    }

    .btn-purchase:active:not(:disabled) {
      transform: translateY(0);
    }

    .btn-purchase:disabled {
      opacity: 0.55;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .card {
        padding: 28px 22px;
      }
      .card-title {
        font-size: 1.3rem;
      }
      .card-description {
        font-size: 0.92rem;
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

  getInsuranceType(): string {
    const type = this.insuranceTypeName?.toLowerCase() || this.title?.toLowerCase() || '';
    if (type.includes('vehicle') || type.includes('auto') || type.includes('car')) return 'vehicle';
    if (type.includes('home') || type.includes('property') || type.includes('house')) return 'home';
    if (type.includes('health') || type.includes('medical')) return 'health';
    if (type.includes('travel')) return 'travel';
    return 'travel';
  }
}
