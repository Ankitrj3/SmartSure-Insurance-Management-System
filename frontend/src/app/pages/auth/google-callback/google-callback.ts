import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './google-callback.html',
  styleUrl: './google-callback.css',
})
export class GoogleCallback implements OnInit {
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const role = params['role'];
      const errorStr = params['error'];
      
      if (errorStr) {
         this.error = errorStr;
         this.loading = false;
         return;
      }
      
      if (token) {
        localStorage.setItem('smartsure_auth_token', token);
        localStorage.setItem('smartsure_auth_role', role || 'User');
        
        // Navigate based on role
        if (role === 'Admin' || role === 'admin') {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/user/dashboard']);
        }
      } else {
         this.error = 'No token received from Server.';
         this.loading = false;
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
