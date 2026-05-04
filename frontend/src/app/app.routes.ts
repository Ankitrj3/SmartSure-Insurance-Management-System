import { Routes } from '@angular/router';
import { Home } from './pages/home/home';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { userGuard } from './core/guards/user.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'about-us', loadComponent: () => import('./pages/about-us/about-us').then(m => m.AboutUs) },
  { path: 'contact-us', loadComponent: () => import('./pages/contact-us/contact-us').then(m => m.ContactUs) },
  { path: 'insurance', loadComponent: () => import('./pages/insurance/insurance').then(m => m.Insurance) },
  { path: 'login', loadComponent: () => import('./pages/auth/login/login').then(m => m.Login) },
  { path: 'forgot-password', loadComponent: () => import('./pages/auth/forgot-password/forgot-password').then(m => m.ForgotPassword) },
  { path: 'register', loadComponent: () => import('./pages/auth/register/register').then(m => m.Register) },
  { path: 'verify-otp', loadComponent: () => import('./pages/auth/verify-otp/verify-otp').then(m => m.VerifyOtp) },
  { path: 'profile', loadComponent: () => import('./pages/profile/profile').then(m => m.Profile), canActivate: [authGuard] },
  { path: 'user/dashboard', loadComponent: () => import('./pages/user-dashboard/user-dashboard').then(m => m.UserDashboard), canActivate: [userGuard] },
  { path: 'user-dashboard', redirectTo: 'user/dashboard', pathMatch: 'full' },
  { path: 'admin/dashboard', loadComponent: () => import('./pages/admin-dashboard/admin-dashboard').then(m => m.AdminDashboard), canActivate: [adminGuard] },
  { path: 'auth/google/callback', loadComponent: () => import('./pages/auth/google-callback/google-callback').then(m => m.GoogleCallback) },
  { path: '404', loadComponent: () => import('./pages/not-found/not-found').then(m => m.NotFound) },
  { path: '**', redirectTo: '/404' }
];
