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

    return `
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Policy Invoice - ${policy.policyId}</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { 
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
      padding: 40px; 
      background: #f5f5f5;
    }
    .invoice-container {
      max-width: 800px;
      margin: 0 auto;
      background: white;
      padding: 40px;
      box-shadow: 0 0 20px rgba(0,0,0,0.1);
    }
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 3px solid #0DB18C;
      padding-bottom: 20px;
      margin-bottom: 30px;
    }
    .logo {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .logo-icon {
      width: 50px;
      height: 50px;
      background: linear-gradient(135deg, #0DB18C 0%, #0a9170 100%);
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 24px;
      font-weight: bold;
    }
    .logo-text {
      font-size: 28px;
      font-weight: bold;
      color: #0DB18C;
    }
    .invoice-title {
      text-align: right;
    }
    .invoice-title h1 {
      font-size: 32px;
      color: #333;
      margin-bottom: 5px;
    }
    .invoice-title p {
      color: #666;
      font-size: 14px;
    }
    .section {
      margin-bottom: 30px;
    }
    .section-title {
      font-size: 18px;
      font-weight: bold;
      color: #0DB18C;
      margin-bottom: 15px;
      padding-bottom: 8px;
      border-bottom: 2px solid #e0e0e0;
    }
    .info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 15px;
    }
    .info-item {
      display: flex;
      flex-direction: column;
    }
    .info-label {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 5px;
    }
    .info-value {
      font-size: 15px;
      color: #333;
      font-weight: 500;
    }
    .highlight-box {
      background: linear-gradient(135deg, #0DB18C 0%, #0a9170 100%);
      color: white;
      padding: 20px;
      border-radius: 10px;
      margin: 20px 0;
    }
    .highlight-box .amount {
      font-size: 36px;
      font-weight: bold;
      margin-bottom: 5px;
    }
    .highlight-box .label {
      font-size: 14px;
      opacity: 0.9;
    }
    .terms-list {
      list-style: none;
      padding: 0;
    }
    .terms-list li {
      padding: 10px 0;
      padding-left: 25px;
      position: relative;
      line-height: 1.6;
      color: #555;
    }
    .terms-list li:before {
      content: "✓";
      position: absolute;
      left: 0;
      color: #0DB18C;
      font-weight: bold;
    }
    .warning-box {
      background: #fff3cd;
      border-left: 4px solid #ffc107;
      padding: 15px;
      margin: 20px 0;
      border-radius: 5px;
    }
    .warning-box strong {
      color: #856404;
      display: block;
      margin-bottom: 8px;
    }
    .warning-box ul {
      margin-left: 20px;
      color: #856404;
    }
    .footer {
      margin-top: 40px;
      padding-top: 20px;
      border-top: 2px solid #e0e0e0;
      text-align: center;
      color: #666;
      font-size: 12px;
    }
    @media print {
      body { padding: 0; background: white; }
      .invoice-container { box-shadow: none; }
    }
  </style>
</head>
<body>
  <div class="invoice-container">
    <!-- Header -->
    <div class="header">
      <div class="logo">
        <div class="logo-icon">S</div>
        <div class="logo-text">SMARTSURE</div>
      </div>
      <div class="invoice-title">
        <h1>POLICY INVOICE</h1>
        <p>Date: ${currentDate}</p>
      </div>
    </div>

    <!-- Policy Information -->
    <div class="section">
      <div class="section-title">Policy Information</div>
      <div class="info-grid">
        <div class="info-item">
          <div class="info-label">Policy ID</div>
          <div class="info-value">${policy.policyId}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Policy Type</div>
          <div class="info-value">${policy.typeName} - ${policy.subtypeName}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Start Date</div>
          <div class="info-value">${new Date(policy.startDate).toLocaleDateString('en-IN')}</div>
        </div>
        <div class="info-item">
          <div class="info-label">End Date</div>
          <div class="info-value">${new Date(policy.endDate).toLocaleDateString('en-IN')}</div>
        </div>
        <div class="info-item">
          <div class="info-label">Status</div>
          <div class="info-value">${policy.status}</div>
        </div>
        ${policy.nomineeName ? `
        <div class="info-item">
          <div class="info-label">Nominee</div>
          <div class="info-value">${policy.nomineeName} (${policy.nomineeRelation || 'N/A'})</div>
        </div>
        ` : ''}
      </div>
    </div>

    ${isVehicle ? this.getVehicleDetailsHTML(details) : this.getHomeDetailsHTML(details)}

    <!-- Financial Details -->
    <div class="highlight-box">
      <div class="info-grid" style="color: white;">
        <div>
          <div class="label">Insured Declared Value (IDV)</div>
          <div class="amount">₹${policy.insuredDeclaredValue?.toLocaleString('en-IN')}</div>
        </div>
        <div style="text-align: right;">
          <div class="label">Annual Premium</div>
          <div class="amount">₹${policy.premiumAmount?.toLocaleString('en-IN')}</div>
        </div>
      </div>
    </div>

    <!-- Claim Policy & Terms -->
    <div class="section">
      <div class="section-title">Claim Policy & Terms</div>
      <ul class="terms-list">
        <li><strong>Maximum Claims:</strong> You can file up to 3 approved claims per policy period.</li>
        <li><strong>Claim Limits:</strong> ${isVehicle ? 
          'For accidents, maximum claim is 75% of IDV. For theft/total loss, full IDV is covered.' : 
          'Claims are subject to policy terms and actual damage assessment.'}</li>
        <li><strong>Theft Claims:</strong> ${isVehicle ? 
          'In case of vehicle theft, you will receive the full IDV amount and the policy will be terminated.' : 
          'Theft claims require police FIR and investigation.'}</li>
        <li><strong>Document Requirements:</strong> All claims must be supported by proper documentation including photos, bills, and police reports where applicable.</li>
        <li><strong>Claim Processing:</strong> Claims are typically processed within 7-15 business days after document verification.</li>
        <li><strong>Policy Validity:</strong> Ensure timely premium payment to keep your policy active.</li>
      </ul>
    </div>

    <!-- Important Warnings -->
    <div class="warning-box">
      <strong>⚠ Important Warnings</strong>
      <ul>
        <li>Filing false or fraudulent claims may result in policy cancellation and legal action.</li>
        <li>Three consecutive rejected claims due to false information will result in automatic policy termination.</li>
        <li>Always report incidents to authorities (police/fire department) within 24 hours.</li>
        <li>Modifications to ${isVehicle ? 'vehicle' : 'property'} must be reported to maintain coverage.</li>
      </ul>
    </div>

    <!-- Footer -->
    <div class="footer">
      <p><strong>SmartSure Insurance Platform</strong></p>
      <p>For claims and support, contact: support@smartsure.com | 1800-XXX-XXXX</p>
      <p>This is a computer-generated invoice and does not require a signature.</p>
      <p style="margin-top: 10px;">© ${new Date().getFullYear()} SmartSure. All rights reserved.</p>
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
