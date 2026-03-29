import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './components/navbar/navbar';
import { FooterComponent } from './components/footer/footer.component';
import { AuthService } from './core/services/auth.service';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, FooterComponent, ToastComponent],
  template: `
    <app-navbar></app-navbar>
    <app-toast></app-toast>
    <main class="page-content">
      <router-outlet></router-outlet>
    </main>
    <app-footer></app-footer>
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
