import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './components/navbar/navbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar],
  template: `
    <app-navbar></app-navbar>
    <main class="page-content">
      <router-outlet></router-outlet>
    </main>
    <footer class="smartsure-footer">
      <div class="footer-container">
        <div class="footer-brand">
          <div class="logo">
            <span class="logo-shield">🛡️</span>
            <span class="logo-text">Smart<span class="highlight">Sure</span></span>
          </div>
          <p>Next-generation insurance solutions delivering real-time compensation, dynamic planning, and total peace of mind for you and your family.</p>
          <div class="social-links">
            <a href="#" class="social-icon">Tw</a>
            <a href="#" class="social-icon">Li</a>
            <a href="#" class="social-icon">Ig</a>
          </div>
        </div>
        
        <div class="footer-links">
          <h4>Coverage</h4>
          <ul>
            <li><a href="#">Health Guard</a></li>
            <li><a href="#">Life Protection</a></li>
            <li><a href="#">Vehicle & Motor</a></li>
            <li><a href="#">Property Defend</a></li>
          </ul>
        </div>
        
        <div class="footer-links">
          <h4>SmartSure</h4>
          <ul>
            <li><a href="#">About Us</a></li>
            <li><a href="#">Our Network</a></li>
            <li><a href="#">Careers</a></li>
            <li><a href="#">Partner with us</a></li>
          </ul>
        </div>
        
        <div class="footer-links">
          <h4>Support & Legal</h4>
          <ul>
            <li><a href="#">File a Claim</a></li>
            <li><a href="#">Privacy Policy</a></li>
            <li><a href="#">Terms & Conditions</a></li>
            <li><a href="#">Contact Support</a></li>
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
