import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  registerData = {
    fullName: '',
    email: '',
    phoneNumber: '',
    password: ''
  };

  otpCode = '';
  verificationMode = false;
  
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  onRegister() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.register(this.registerData).subscribe({
      next: (res) => {
        this.isLoading = false;
        // Redirect to separate verify-otp page
        this.router.navigate(['/verify-otp'], { queryParams: { email: this.registerData.email } });
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to register account.';
      }
    });
  }

  onVerify() {
    // This internal method is kept for backwards compatibility or if modal is used
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const dt = {
      email: this.registerData.email,
      otp: this.otpCode
    };

    this.authService.verifyOtp(dt).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid OTP code.';
      }
    });
  }

  loginWithGoogle() {
    const url = this.authService.getGoogleLoginUrl();
    window.location.href = url;
  }
}
