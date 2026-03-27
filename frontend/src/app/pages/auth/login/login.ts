import { Component, OnInit } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {
  credentials = { email: '', password: '' };
  isLoading = false;
  errorMessage = '';
  returnUrl: string = '';

  constructor(
    private authService: AuthService, 
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Get return URL from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    
    // If already logged in, redirect based on role
    if (this.authService.hasToken()) {
      const role = this.authService.getRole();
      if (role === 'Admin' || role === 'admin') {
        this.router.navigate(['/admin/dashboard']);
      } else {
        this.router.navigate(['/user/dashboard']);
      }
    }
  }

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.credentials).subscribe({
      next: (res) => {
        this.isLoading = false;
        
        // If there's a return URL and it's not login/register, use it
        if (this.returnUrl && this.returnUrl !== '/' && !this.returnUrl.includes('login') && !this.returnUrl.includes('register')) {
          this.router.navigateByUrl(this.returnUrl);
        } else {
          // Otherwise redirect based on role
          if (res.role === 'Admin' || res.role === 'admin') {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/user/dashboard']);
          }
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
