import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PdfService {

  generatePolicyInvoice(policy: any, policyDetails: any) {
    const isVehicle = policy.typeName?.toLowerCase().includes('vehicle');
    
    // Create PDF content as HTML
    const content = this.createInvoiceHTML(policy, policyDetails, isVehicle);
    
    // Open print dialog
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(content);
      printWindow.document.close();
      printWindow.focus();
      
      // Auto-print after a short delay
      setTimeout(() => {
        printWindow.print();
      }, 500);
    }
  }

  private createInvoiceHTML(policy: any, details: any, isVehicle: boolean): string {
    const currentDate = new Date().toLocaleDateString('en-IN', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });

    const policyStartDate = new Date(policy.startDate).toLocaleDateString('en-IN', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });

    const policyEndDate = new Date(policy.endDate).toLocaleDateString('en-IN', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });

    return `
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Policy Certificate - ${policy.policyId}</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { 
      font-family: 'Arial', 'Helvetica', sans-serif; 
      padding: 30px; 
      background: white;
      color: #333;
      line-height: 1.6;
    }
    .certificate-container {
      max-width: 900px;
      margin: 0 auto;
      background: white;
      border: 3px solid #0DB18C;
      padding: 0;
    }
    .header-bar {
      background: linear-gradient(135deg, #0DB18C 0%, #0a9170 100%);
      padding: 25px 40px;
      color: white;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .logo-section {
      display: flex;
      align-items: center;
      gap: 15px;
    }
    .logo-shield {
      width: 60px;
      height: 60px;
      background: white;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 32px;
      font-weight: bold;
      color: #0DB18C;
    }
    .company-info h1 {
      font-size: 28px;
      font-weight: bold;
      margin-bottom: 3px;
      letter-spacing: 1px;
    }
    .company-info p {
      font-size: 12px;
      opacity: 0.95;
      margin: 0;
    }
    .cert-number {
      text-align: right;
    }
    .cert-number h2 {
      font-size: 16px;
      font-weight: normal;
      opacity: 0.9;
      margin-bottom: 5px;
    }
    .cert-number .number {
      font-size: 14px;
      font-family: 'Courier New', monospace;
      background: rgba(255,255,255,0.2);
      padding: 5px 10px;
      border-radius: 4px;
    }
    .content-area {
      padding: 40px;
    }
    .certificate-title {
      text-align: center;
      margin-bottom: 30px;
      padding-bottom: 20px;
      border-bottom: 2px solid #e0e0e0;
    }
    .certificate-title h2 {
      font-size: 32px;
      color: #0DB18C;
      margin-bottom: 8px;
      font-weight: bold;
    }
    .certificate-title p {
      font-size: 14px;
      color: #666;
    }
    .section {
      margin-bottom: 30px;
    }
    .section-title {
      font-size: 16px;
      font-weight: bold;
      color: #0DB18C;
      margin-bottom: 15px;
      padding-bottom: 8px;
      border-bottom: 2px solid #0DB18C;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px 30px;
      margin-bottom: 20px;
    }
    .info-item {
      display: flex;
      flex-direction: column;
    }
    .info-label {
      font-size: 11px;
      color: #666;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 5px;
      font-weight: 600;
    }
    .info-value {
      font-size: 15px;
      color: #000;
      font-weight: 500;
    }
    .coverage-box {
      background: #f8f9fa;
      border-left: 4px solid #0DB18C;
      padding: 25px;
      margin: 25px 0;
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }
    .coverage-item {
      text-align: center;
    }
    .coverage-label {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 8px;
      font-weight: 600;
    }
    .coverage-amount {
      font-size: 28px;
      font-weight: bold;
      color: #0DB18C;
    }
    .terms-section {
      background: #f8f9fa;
      padding: 25px;
      border-radius: 8px;
      margin: 25px 0;
    }
    .terms-list {
      list-style: none;
      padding: 0;
      margin: 15px 0 0 0;
    }
    .terms-list li {
      padding: 10px 0;
      padding-left: 30px;
      position: relative;
      font-size: 13px;
      color: #444;
      line-height: 1.6;
    }
    .terms-list li:before {
      content: "✓";
      position: absolute;
      left: 0;
      color: #0DB18C;
      font-weight: bold;
      font-size: 16px;
    }
    .warning-box {
      background: white;
      border: 2px solid #d32f2f;
      border-radius: 4px;
      margin: 30px 0;
      overflow: hidden;
    }
    .warning-header {
      background: #d32f2f;
      color: white;
      padding: 15px 25px;
      display: flex;
      align-items: center;
      gap: 15px;
    }
    .warning-icon {
      width: 35px;
      height: 35px;
      background: white;
      color: #d32f2f;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 24px;
      font-weight: bold;
      flex-shrink: 0;
    }
    .warning-title {
      font-size: 16px;
      font-weight: bold;
      letter-spacing: 1px;
    }
    .warning-content {
      padding: 25px;
      background: #fafafa;
    }
    .warning-item {
      padding: 15px;
      margin-bottom: 12px;
      background: white;
      border-left: 4px solid #d32f2f;
      font-size: 13px;
      line-height: 1.7;
      color: #333;
    }
    .warning-item:last-child {
      margin-bottom: 0;
    }
    .warning-item strong {
      color: #d32f2f;
      display: block;
      margin-bottom: 6px;
      font-size: 13px;
    }
    .footer-section {
      margin-top: 40px;
      padding-top: 25px;
      border-top: 2px solid #e0e0e0;
    }
    .footer-info {
      text-align: center;
      padding: 25px;
      background: #f8f9fa;
      border-radius: 8px;
    }
    .footer-info p {
      font-size: 11px;
      color: #666;
      margin: 5px 0;
    }
    .footer-info strong {
      color: #0DB18C;
      font-size: 12px;
    }
    .watermark {
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%) rotate(-45deg);
      font-size: 120px;
      color: rgba(13, 177, 140, 0.05);
      font-weight: bold;
      z-index: -1;
      pointer-events: none;
    }
    @media print {
      body { padding: 0; background: white; }
      .certificate-container { border: 2px solid #0DB18C; }
    }
  </style>
</head>
<body>
  <div class="watermark">SMARTSURE</div>
  <div class="certificate-container">
    <!-- Header -->
    <div class="header-bar">
      <div class="logo-section">
        <div class="logo-shield">S</div>
        <div class="company-info">
          <h1>SMARTSURE</h1>
          <p>Insurance Management Platform</p>
          <p>IRDAI Registration No: IRDA/DB/2024/001</p>
        </div>
      </div>
      <div class="cert-number">
        <h2>Policy Certificate</h2>
        <div class="number">${policy.policyId.substring(0, 13).toUpperCase()}</div>
        <p style="margin-top: 8px; font-size: 11px;">Issue Date: ${currentDate}</p>
      </div>
    </div>

    <!-- Content -->
    <div class="content-area">
      <div class="certificate-title">
        <h2>INSURANCE POLICY CERTIFICATE</h2>
        <p>This certifies that the policy holder is covered under the terms and conditions stated herein</p>
      </div>

      <!-- Policy Information -->
      <div class="section">
        <div class="section-title">Policy Details</div>
        <div class="info-grid">
          <div class="info-item">
            <div class="info-label">Policy Number</div>
            <div class="info-value">${policy.policyId.substring(0, 13).toUpperCase()}</div>
          </div>
          <div class="info-item">
            <div class="info-label">Policy Type</div>
            <div class="info-value">${policy.typeName} Insurance</div>
          </div>
          <div class="info-item">
            <div class="info-label">Plan</div>
            <div class="info-value">${policy.subtypeName}</div>
          </div>
          <div class="info-item">
            <div class="info-label">Policy Status</div>
            <div class="info-value" style="color: #0DB18C;">${policy.status}</div>
          </div>
          <div class="info-item">
            <div class="info-label">Period of Insurance</div>
            <div class="info-value">${policyStartDate}</div>
          </div>
          <div class="info-item">
            <div class="info-label">Policy Expiry Date</div>
            <div class="info-value">${policyEndDate}</div>
          </div>
          ${policy.nomineeName ? `
          <div class="info-item">
            <div class="info-label">Nominee Name</div>
            <div class="info-value">${policy.nomineeName}</div>
          </div>
          <div class="info-item">
            <div class="info-label">Nominee Relationship</div>
            <div class="info-value">${policy.nomineeRelation || 'Not Specified'}</div>
          </div>
          ` : ''}
        </div>
      </div>

      ${isVehicle ? this.getVehicleDetailsHTML(details) : this.getHomeDetailsHTML(details)}

      <!-- Coverage Amount -->
      <div class="coverage-box">
        <div class="coverage-item">
          <div class="coverage-label">Sum Insured (IDV)</div>
          <div class="coverage-amount">₹${policy.insuredDeclaredValue?.toLocaleString('en-IN')}</div>
        </div>
        <div class="coverage-item">
          <div class="coverage-label">Annual Premium</div>
          <div class="coverage-amount">₹${policy.premiumAmount?.toLocaleString('en-IN')}</div>
        </div>
      </div>

      <!-- Terms & Conditions -->
      <div class="terms-section">
        <div class="section-title">Coverage Terms & Claim Policy</div>
        <ul class="terms-list">
          <li><strong>Claim Limit:</strong> Maximum of 3 approved claims allowed during the policy period.</li>
          <li><strong>Claim Amount:</strong> ${isVehicle ? 
            'For accidents, up to 75% of IDV. For theft or total loss, full IDV amount is payable.' : 
            'Claims are subject to actual loss assessment and policy terms.'}</li>
          <li><strong>Theft/Total Loss:</strong> ${isVehicle ? 
            'In case of vehicle theft, full IDV will be paid and policy will be terminated.' : 
            'Requires police FIR and investigation report.'}</li>
          <li><strong>Documentation:</strong> All claims must be supported by proper documentation including photographs, repair bills, and police reports where applicable.</li>
          <li><strong>Claim Processing:</strong> Claims are typically processed within 7-15 business days after document verification and approval.</li>
          <li><strong>Premium Payment:</strong> Timely premium payment is mandatory to keep the policy active and maintain coverage.</li>
          <li><strong>Modifications:</strong> Any modifications to the ${isVehicle ? 'vehicle' : 'property'} must be reported to maintain valid coverage.</li>
        </ul>
      </div>

      <!-- Important Notices -->
      <div class="warning-box">
        <div class="warning-header">
          <div class="warning-icon">!</div>
          <div class="warning-title">IMPORTANT POLICY CONDITIONS</div>
        </div>
        <div class="warning-content">
          <div class="warning-item">
            <strong>1. Fraudulent Claims:</strong> Any attempt to file false or fraudulent claims will result in immediate policy cancellation, forfeiture of all premiums paid, and may lead to legal prosecution under applicable insurance laws and regulations.
          </div>
          <div class="warning-item">
            <strong>2. Claim Rejection Policy:</strong> If three consecutive claims are rejected due to submission of false information or fraudulent documentation, the policy will be automatically terminated without any refund of premiums.
          </div>
          <div class="warning-item">
            <strong>3. Incident Reporting:</strong> All incidents, accidents, or losses must be reported to the relevant authorities (police, fire department, or other applicable agencies) within 24 hours of occurrence. Failure to do so may result in claim rejection.
          </div>
          <div class="warning-item">
            <strong>4. Policy Terms:</strong> This insurance policy is governed by the terms and conditions outlined in the policy document. The policyholder is advised to read and understand all terms, conditions, exclusions, and limitations before filing any claim.
          </div>
          <div class="warning-item">
            <strong>5. Premium Payment:</strong> Timely payment of premiums is mandatory. Non-payment or delayed payment beyond the grace period will result in policy lapse and loss of coverage. No claims will be entertained during the lapsed period.
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="footer-section">
        <div class="footer-info">
          <p><strong>SmartSure Insurance Management Platform</strong></p>
          <p>Registered Office: SmartSure Tower, Insurance District, Mumbai - 400001, India</p>
          <p>Customer Care: 1800-XXX-XXXX | Email: support@smartsure.com | Web: www.smartsure.com</p>
          <p style="margin-top: 15px; font-size: 11px; font-weight: 600; color: #0DB18C;">This is a digitally generated certificate and is valid without physical signature.</p>
          <p style="font-size: 10px; margin-top: 5px;">© ${new Date().getFullYear()} SmartSure. All rights reserved. | IRDAI Reg. No: IRDA/DB/2024/001</p>
        </div>
      </div>
    </div>
  </div>
</body>
</html>
    `;
  }

  private getVehicleDetailsHTML(details: any): string {
    return `
    <div class="section">
      <div class="section-title">Vehicle Details</div>
      <div class="info-grid">
        <div class="info-item">
          <div class="info-label">Registration Number</div>
          <div class="info-value">${details.registrationNumber || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Make & Model</div>
          <div class="info-value">${details.make || 'N/A'} ${details.model || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Manufacture Year</div>
          <div class="info-value">${details.manufactureYear || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Ex-Showroom Price</div>
          <div class="info-value">₹${details.estimatedValue?.toLocaleString('en-IN') || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Chassis Number</div>
          <div class="info-value">${details.chassisNumber || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Engine Number</div>
          <div class="info-value">${details.engineNumber || 'N/A'}</div>
        </div>
      </div>
    </div>
    `;
  }

  private getHomeDetailsHTML(details: any): string {
    return `
    <div class="section">
      <div class="section-title">Property Details</div>
      <div class="info-grid">
        <div class="info-item">
          <div class="info-label">Property Address</div>
          <div class="info-value">${details.address || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Property Type</div>
          <div class="info-value">${details.propertyType || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Year Built</div>
          <div class="info-value">${details.yearBuilt || 'N/A'}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Estimated Value</div>
          <div class="info-value">₹${details.estimatedValue?.toLocaleString('en-IN') || 'N/A'}</div>
        </div>
        ${details.securityFeatures ? `
        <div class="info-item" style="grid-column: 1 / -1;">
          <div class="info-label">Security Features</div>
          <div class="info-value">${details.securityFeatures}</div>
        </div>
        ` : ''}
      </div>
    </div>
    `;
  }
}
