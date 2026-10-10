import { Injectable, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';

export const authenticationGuard: CanActivateFn = (): boolean => {
  const authState: AuthenticationStateService = inject(AuthenticationStateService);
  const router: Router = inject(Router);

  if (!authState.isAuthenticated()) {
    router.navigate(['/auth/login']);
    return false;
  }
  return true;
};
