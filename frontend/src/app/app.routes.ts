import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Login } from './pages/auth/login/login';
import { Register } from './pages/auth/register/register';
import { UserDashboard } from './pages/user-dashboard/user-dashboard';
import { AdminDashboard } from './pages/admin-dashboard/admin-dashboard';
import { GoogleCallback } from './pages/auth/google-callback/google-callback';
import { VerifyOtp } from './pages/auth/verify-otp/verify-otp';
import { ForgotPassword } from './pages/auth/forgot-password/forgot-password';

import { AboutUs } from './pages/about-us/about-us';
import { ContactUs } from './pages/contact-us/contact-us';
import { Insurance } from './pages/insurance/insurance';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { userGuard } from './core/guards/user.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'about-us', component: AboutUs },
  { path: 'contact-us', component: ContactUs },
  { path: 'insurance', component: Insurance },
  { path: 'login', component: Login },
  { path: 'forgot-password', component: ForgotPassword },
  { path: 'register', component: Register },
  { path: 'verify-otp', component: VerifyOtp },
  { path: 'user/dashboard', component: UserDashboard, canActivate: [userGuard] },
  { path: 'user-dashboard', redirectTo: 'user/dashboard', pathMatch: 'full' },
  { path: 'admin/dashboard', component: AdminDashboard, canActivate: [adminGuard] },
  { path: 'auth/google/callback', component: GoogleCallback }
];
