import { Injectable, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';

export const guestGuard: CanActivateFn = (): boolean => {
  const authState: AuthenticationStateService = inject(AuthenticationStateService);
  const router: Router = inject(Router);

  if (authState.isAuthenticated()) {
    const role: string | null = authState.role();
    switch (role) {
      case 'Admin':
        router.navigate(['/admin']);
        break;
      case 'Employee':
        router.navigate(['/employee']);
        break;
      case 'Evaluator':
        router.navigate(['/evaluator']);
        break;
      default:
        router.navigate(['/dashboard']);
        break;
    }
    return false;
  }
  return true;
};
