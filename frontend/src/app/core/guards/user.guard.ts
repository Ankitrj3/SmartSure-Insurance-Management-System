import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const userGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getToken();
  
  if (!token) {
    // Not logged in, show 404
    router.navigate(['/404']);
    return false;
  }

  const role = authService.getRole();
  
  // Allow User, Customer, and Admin roles
  if (role === 'User' || role === 'user' || role === 'Customer' || role === 'customer' || role === 'Admin' || role === 'admin') {
    return true;
  }

  // Unknown role, redirect to 404
  router.navigate(['/404']);
  return false;
};
