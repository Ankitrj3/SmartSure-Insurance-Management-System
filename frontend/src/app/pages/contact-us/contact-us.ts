import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-container">
      <div class="header-banner">
        <h1>Contact Us</h1>
        <p>We're here to help you 24/7.</p>
      </div>
      <div class="contact-grid">
        <div class="contact-card">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="contact-icon"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path></svg>
          <h3>Phone</h3>
          <p>+1 (800) 123-4567<br>Support Team</p>
        </div>
        <div class="contact-card">
           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="contact-icon"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
          <h3>Email</h3>
          <p>support&#64;smartsure.com<br>Response within 2 hours</p>
        </div>
        <div class="contact-card">
           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="contact-icon"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>
          <h3>Office</h3>
          <p>123 Insurance Blvd<br>New York, NY 10001</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container {
      padding: 60px 5%;
      background: #F7F9FC;
      min-height: 80vh;
    }
    .header-banner {
      text-align: center;
      margin-bottom: 60px;
    }
    .header-banner h1 {
      font-size: 3rem;
      color: #10B981;
      margin-bottom: 15px;
    }
    .header-banner p {
      font-size: 1.2rem;
      color: #64748b;
    }
    .contact-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 30px;
      max-width: 1100px;
      margin: 0 auto;
    }
    .contact-card {
      background: white;
      padding: 40px 30px;
      border-radius: 16px;
      text-align: center;
      box-shadow: 0 4px 15px rgba(0,0,0,0.05);
      border-top: 4px solid #10B981;
      transition: transform 0.3s;
    }
    .contact-card:hover {
      transform: translateY(-5px);
    }
    .contact-icon {
      width: 48px;
      height: 48px;
      color: #10B981;
      margin-bottom: 20px;
    }
    .contact-card h3 {
      font-size: 1.5rem;
      margin-bottom: 10px;
      color: #1A1F36;
    }
    .contact-card p {
      color: #4F566B;
      line-height: 1.6;
    }
  `]
})
export class ContactUs {}
