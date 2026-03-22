import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  credentials = { email: '', password: '' };
  isLoading = false;
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.credentials).subscribe({
      next: (res) => {
        this.isLoading = false;
        // Redirect to dashboard internally based on role
        if (res.role === 'Admin') {
            this.router.navigate(['/admin/dashboard']);
        } else {
            this.router.navigate(['/user/dashboard']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid email or password.';
      }
    });
  }

  loginWithGoogle() {
    // Redirect browser to backend Google entry point
    const url = this.authService.getGoogleLoginUrl();
    window.location.href = url;
  }
}
