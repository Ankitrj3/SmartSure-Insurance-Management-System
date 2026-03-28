import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { Navbar } from './components/navbar/navbar';
import { AuthService } from './core/services/auth.service';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, RouterLink, ToastComponent],
  template: `
    <app-navbar></app-navbar>
    <app-toast></app-toast>
    <main class="page-content">
      <router-outlet></router-outlet>
    </main>
    <footer class="smartsure-footer">
      <div class="footer-container">
        <div class="footer-brand">
          <div class="logo">
            <svg viewBox="0 0 24 24" fill="none" class="footer-logo-icon">
              <path d="M12 2L3 6v6.22c0 5.51 3.84 10.74 9 12 5.16-1.26 9-6.49 9-12V6l-9-4z" fill="#0DB18C"/>
              <path d="M12 2v20c5.16-1.26 9-6.49 9-12V6l-9-4z" fill="#0A8E70"/>
            </svg>
            <span class="logo-text">SMARTSURE</span>
          </div>
          <p>
            Explore our tailored insurance solutions and experience the peace of mind that comes with knowing you're covered.
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
export class AppComponent implements OnInit {
  title = 'smartsure-frontend';

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Initialize auth state from localStorage on app startup
    const token = this.authService.getToken();
    const role = this.authService.getRole();
    
    if (token && role) {
      console.log('Auth state restored from localStorage:', { role });
    }
  }
}
