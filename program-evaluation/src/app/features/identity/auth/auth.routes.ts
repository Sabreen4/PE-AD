import { Routes } from '@angular/router';
import { guestGuard } from '@core/guards/guest.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then((m) => m.LoginComponent),
    canActivate: [guestGuard],
    title: 'Login \u2014 PE&DA',
  },
  {
    path: 'verify-otp',
    loadComponent: () => import('./verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent),
    canActivate: [guestGuard],
    title: 'Verify OTP \u2014 PE&DA',
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
];
