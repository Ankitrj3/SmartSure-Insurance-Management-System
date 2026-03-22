import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about-us',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-container">
      <div class="header-banner">
        <h1>About SmartSure</h1>
        <p>Your Trusted Partner in Complete Protection.</p>
      </div>
      <div class="content-body">
        <div class="info-card">
          <h2>Our Mission</h2>
          <p>At SmartSure, our mission is to provide transparent, affordable, and comprehensive insurance policies tailored to individual needs. We believe in empowering our clients with digital tools that make managing insurance simple and stress-free.</p>
        </div>
        <div class="info-card">
          <h2>Our Vision</h2>
          <p>We envision a world where everyone has access to top-tier financial security and peace of mind. Through innovation and exceptional customer service, we are redefining the insurance industry.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container {
      padding: 60px 5%;
      background: white;
      min-height: 80vh;
    }
    .header-banner {
      text-align: center;
      margin-bottom: 50px;
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
    .content-body {
      max-width: 900px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 30px;
    }
    .info-card {
      background: #F0FDF4;
      padding: 40px;
      border-radius: 16px;
      border-left: 5px solid #10B981;
      box-shadow: 0 4px 6px rgba(0,0,0,0.05);
    }
    .info-card h2 {
      color: #1A1F36;
      margin-bottom: 15px;
    }
    .info-card p {
      color: #4F566B;
      line-height: 1.6;
    }
  `]
})
export class AboutUs {}
