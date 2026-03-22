import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about-us',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-container">
      <div class="header-banner">
        <span class="badge-yellow">KNOW US</span>
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
      padding: 100px 5%;
      background: white;
      min-height: 80vh;
      font-family: 'Inter', sans-serif;
    }
    .header-banner {
      text-align: center;
      margin-bottom: 60px;
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
    .header-banner h1 {
      font-size: 3.5rem;
      color: #1A1A1A;
      margin-bottom: 15px;
      font-weight: 800;
    }
    .header-banner p {
      font-size: 1.25rem;
      color: #666666;
    }
    .content-body {
      max-width: 900px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 30px;
    }
    .info-card {
      background: #F8F9FA;
      padding: 40px;
      border-radius: 0px;
      border-left: 5px solid #0DB18C;
      box-shadow: 0 4px 15px rgba(0,0,0,0.05);
    }
    .info-card h2 {
      color: #0DB18C;
      margin-bottom: 15px;
      font-size: 2rem;
      font-weight: 700;
    }
    .info-card p {
      color: #666666;
      line-height: 1.6;
      font-size: 1.1rem;
    }
  `]
})
export class AboutUs {}
