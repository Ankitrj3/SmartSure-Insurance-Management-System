import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const userGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getToken();
  
  if (!token) {
    // Not logged in, redirect to login
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const role = authService.getRole();
  
  if (role === 'User' || role === 'user' || role === 'Admin' || role === 'admin') {
    return true;
  }

  // Unknown role, redirect to login
  router.navigate(['/login']);
  return false;
};
