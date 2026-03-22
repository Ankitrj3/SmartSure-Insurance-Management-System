import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { Navbar } from './components/navbar/navbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, RouterLink],
  template: `
    <app-navbar></app-navbar>
    <main class="page-content">
      <router-outlet></router-outlet>
    </main>
    <footer class="smartsure-footer">
      <div class="footer-container">
        <div class="footer-brand">
          <div class="logo">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="footer-logo-icon">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
            </svg>
            <span class="logo-text">Smart<span class="highlight">Sure</span></span>
          </div>
          <p>
            Next-generation insurance solutions delivering real-time compensation, 
            dynamic planning, and total peace of mind for you and your family.
          </p>
          <div class="social-links">
            <a href="#" class="social-icon">Tw</a>
            <a href="#" class="social-icon">Li</a>
            <a href="#" class="social-icon">In</a>
          </div>
        </div>
        
        <div class="footer-links">
          <h4>Coverage</h4>
          <ul>
            <li><a routerLink="/insurance">Vehicle Insurance</a></li>
            <li><a routerLink="/insurance">Home Protection</a></li>
            <li><a routerLink="/insurance">Life Guard</a></li>
            <li><a routerLink="/insurance">Health Secure</a></li>
          </ul>
        </div>
        
        <div class="footer-links">
          <h4>SmartSure</h4>
          <ul>
            <li><a routerLink="/about-us">About Us</a></li>
            <li><a routerLink="/contact-us">Contact Us</a></li>
            <li><a href="#">Our Network</a></li>
            <li><a href="#">Careers</a></li>
          </ul>
        </div>
        
        <div class="footer-links">
          <h4>Support & Legal</h4>
          <ul>
            <li><a href="#">File a Claim</a></li>
            <li><a href="#">Privacy Policy</a></li>
            <li><a href="#">Terms & Conditions</a></li>
            <li><a routerLink="/contact-us">Contact Support</a></li>
          </ul>
        </div>
      </div>
      <div class="footer-bottom">
        <p>&copy; 2026 SmartSure Insurance Platform. All rights reserved.</p>
      </div>
    </footer>
  `,
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'smartsure-frontend';
}
