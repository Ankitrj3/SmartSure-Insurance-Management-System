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

    const totalPremium = policy.premiumAmount || 0;
    const basePremium = totalPremium / 1.18;
    const cgstAmount = basePremium * 0.09;
    const sgstAmount = basePremium * 0.09;

    return `
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Tax Invoice & Policy Certificate - ${policy.policyId}</title>
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { 
      font-family: 'Inter', sans-serif; 
      padding: 0; 
      background: #f8fafc;
      color: #1e293b;
      line-height: 1.5;
    }
    .print-wrapper {
      max-width: 800px;
      margin: 40px auto;
      background: white;
      box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    }
    .header {
      background: #0DB18C;
      color: white;
      padding: 40px;
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }
    .logo-container {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .logo-box {
      width: 48px;
      height: 48px;
      background: white;
      color: #0DB18C;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 24px;
      font-weight: 700;
      border-radius: 0;
    }
    .company-name {
      font-size: 24px;
      font-weight: 700;
      letter-spacing: -0.5px;
      margin-bottom: 4px;
    }
    .company-tag {
      font-size: 12px;
      opacity: 0.9;
      text-transform: uppercase;
      letter-spacing: 1px;
    }
    .invoice-title {
      text-align: right;
    }
    .invoice-title h1 {
      font-size: 32px;
      font-weight: 700;
      margin-bottom: 8px;
      letter-spacing: -1px;
    }
    .invoice-title p {
      font-size: 14px;
      opacity: 0.9;
    }
    .content {
      padding: 40px;
    }
    .meta-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 40px;
      padding-bottom: 20px;
      border-bottom: 2px solid #e2e8f0;
    }
    .meta-col {
      flex: 1;
    }
    .meta-label {
      font-size: 11px;
      text-transform: uppercase;
      color: #64748b;
      font-weight: 600;
      letter-spacing: 0.5px;
      margin-bottom: 4px;
    }
    .meta-value {
      font-size: 15px;
      font-weight: 600;
      color: #0f172a;
    }
    .section-title {
      font-size: 16px;
      font-weight: 700;
      color: #0f172a;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 20px;
      padding-left: 12px;
      border-left: 4px solid #0DB18C;
    }
    .grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 24px;
      margin-bottom: 40px;
    }
    .grid-item {
      background: #f8fafc;
      padding: 16px;
      border: 1px solid #e2e8f0;
    }
    .table-container {
      margin-bottom: 40px;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th {
      background: #1e293b;
      color: white;
      text-align: left;
      padding: 12px 16px;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
    }
    td {
      padding: 16px;
      border-bottom: 1px solid #e2e8f0;
      font-size: 14px;
      color: #334155;
    }
    td.amount {
      text-align: right;
      font-weight: 600;
      color: #0f172a;
    }
    th.amount {
      text-align: right;
    }
    .total-row {
      background: #f8fafc;
    }
    .total-row td {
      font-weight: 700;
      font-size: 16px;
      color: #0DB18C;
      border-bottom: 2px solid #0DB18C;
    }
    .total-row td.amount {
      font-size: 20px;
    }
    .terms {
      margin-bottom: 40px;
      font-size: 12px;
      color: #475569;
    }
    .terms ul {
      list-style: none;
      margin-top: 12px;
    }
    .terms li {
      margin-bottom: 8px;
      padding-left: 16px;
      position: relative;
    }
    .terms li::before {
      content: '■';
      position: absolute;
      left: 0;
      top: 0;
      color: #0DB18C;
      font-size: 10px;
    }
    .footer {
      border-top: 2px solid #e2e8f0;
      padding-top: 24px;
      display: flex;
      justify-content: space-between;
      font-size: 12px;
      color: #64748b;
    }
    .authorized-sign {
      text-align: right;
      margin-top: -20px;
    }
    .sign-box {
      width: 160px;
      height: 60px;
      border-bottom: 1px solid #94a3b8;
      margin-bottom: 8px;
      margin-left: auto;
      display: flex;
      align-items: flex-end;
      justify-content: center;
      color: #0DB18C;
      font-family: 'Brush Script MT', cursive;
      font-size: 24px;
    }
    @media print {
      body { background: white; padding: 0; }
      .print-wrapper { margin: 0; box-shadow: none; max-width: 100%; border: 1px solid #e2e8f0; }
      .grid-item { border-color: #cbd5e1; }
    }
  </style>
</head>
<body>
  <div class="print-wrapper">
    <!-- Header -->
    <div class="header">
      <div class="logo-container">
        <div class="logo-box">
          <svg viewBox="0 0 32 32" fill="none" style="width: 32px; height: 32px;">
            <path d="M16 0L32 8V24L16 32L0 24V8L16 0Z" fill="#1A1A1A"/>
            <path d="M16 5V27L4 21V11L16 5Z" fill="#0DB18C"/>
            <path d="M16 5L28 11V21L16 27V5Z" fill="#0A8E70"/>
          </svg>
        </div>
        <div>
          <div class="company-name">SMART<span style="color: #1A1A1A;">SURE</span></div>
          <div class="company-tag">Insurance Management</div>
        </div>
      </div>
      <div class="invoice-title">
        <h1>TAX INVOICE</h1>
        <p>Original for Recipient</p>
      </div>
    </div>

    <div class="content">
      <!-- Meta Information -->
      <div class="meta-row">
        <div class="meta-col">
          <div class="meta-label">Policy Number</div>
          <div class="meta-value">${policy.policyId.toUpperCase()}</div>
        </div>
        <div class="meta-col">
          <div class="meta-label">Date of Issue</div>
          <div class="meta-value">${currentDate}</div>
        </div>
        <div class="meta-col">
          <div class="meta-label">IRDAI Reg. No</div>
          <div class="meta-value">IRDA/DB/2024/001</div>
        </div>
        <div class="meta-col" style="text-align: right;">
          <div class="meta-label">Status</div>
          <div class="meta-value" style="color: #0DB18C; text-transform: uppercase;">${policy.status}</div>
        </div>
      </div>

      <!-- Policy Details -->
      <div class="section-title">Coverage Information</div>
      <div class="grid">
        <div class="grid-item">
          <div class="meta-label">Policy Type</div>
          <div class="meta-value">${policy.typeName} Insurance - ${policy.subtypeName}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Period of Insurance</div>
          <div class="meta-value">${policyStartDate} to ${policyEndDate}</div>
        </div>
        ${policy.nomineeName ? `
        <div class="grid-item">
          <div class="meta-label">Nominee Name</div>
          <div class="meta-value">${policy.nomineeName}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Nominee Relationship</div>
          <div class="meta-value">${policy.nomineeRelation || 'N/A'}</div>
        </div>
        ` : ''}
      </div>

      <!-- Specific Details -->
      ${isVehicle ? this.getVehicleDetailsHTML(details) : this.getHomeDetailsHTML(details)}

      <!-- Premium Breakdown -->
      <div class="section-title">Premium Summary</div>
      <div class="table-container">
        <table>
          <thead>
            <tr>
              <th>Description</th>
              <th class="amount">Sum Insured (IDV)</th>
              <th class="amount">Premium Amount</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Basic Premium for ${policy.typeName} Cover</td>
              <td class="amount">₹${policy.insuredDeclaredValue?.toLocaleString('en-IN') || '0'}</td>
              <td class="amount">₹${basePremium.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            </tr>
            <tr>
              <td>CGST (9%)</td>
              <td class="amount">-</td>
              <td class="amount">₹${cgstAmount.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            </tr>
            <tr>
              <td>SGST (9%)</td>
              <td class="amount">-</td>
              <td class="amount">₹${sgstAmount.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            </tr>
            <tr class="total-row">
              <td>Total Payable Premium</td>
              <td class="amount"></td>
              <td class="amount">₹${totalPremium.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Terms -->
      <div class="terms">
        <div class="section-title">Terms & Conditions</div>
        <ul>
          <li>This document serves as both a tax invoice and policy certificate under applicable GST rules.</li>
          <li>Coverage is subject to the realization of the full premium amount.</li>
          <li>For detailed terms, conditions, and exclusions, refer to the master policy document.</li>
          <li>In case of a claim, notify SmartSure within 24 hours of the incident.</li>
          <li>This is a computer-generated document and does not require a physical signature.</li>
        </ul>
      </div>

      <!-- Footer -->
      <div class="authorized-sign">
        <div class="sign-box">SmartSure Auth</div>
        <div class="meta-label">Authorized Signatory</div>
      </div>
      
      <div class="footer">
        <div>
          <strong>SmartSure Insurance Management</strong><br>
          Tower A, Tech Park, Mumbai - 400001
        </div>
        <div style="text-align: right;">
          support@smartsure.com<br>
          1800-SMART-SURE
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
      <div class="section-title">Vehicle Details</div>
      <div class="grid">
        <div class="grid-item">
          <div class="meta-label">Registration Number</div>
          <div class="meta-value">${details.registrationNumber || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Make & Model</div>
          <div class="meta-value">${details.make || 'N/A'} ${details.model || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Engine Number</div>
          <div class="meta-value">${details.engineNumber || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Chassis Number</div>
          <div class="meta-value">${details.chassisNumber || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Manufacture Year</div>
          <div class="meta-value">${details.manufactureYear || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Ex-Showroom Price</div>
          <div class="meta-value">₹${details.estimatedValue?.toLocaleString('en-IN') || 'N/A'}</div>
        </div>
      </div>
    `;
  }

  private getHomeDetailsHTML(details: any): string {
    return `
      <div class="section-title">Property Details</div>
      <div class="grid">
        <div class="grid-item" style="grid-column: span 2;">
          <div class="meta-label">Property Address</div>
          <div class="meta-value">${details.address || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Property Type</div>
          <div class="meta-value">${details.propertyType || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Year Built</div>
          <div class="meta-value">${details.yearBuilt || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Estimated Value</div>
          <div class="meta-value">₹${details.estimatedValue?.toLocaleString('en-IN') || 'N/A'}</div>
        </div>
        <div class="grid-item">
          <div class="meta-label">Security Features</div>
          <div class="meta-value">${details.securityFeatures || 'N/A'}</div>
        </div>
      </div>
    `;
  }
}
