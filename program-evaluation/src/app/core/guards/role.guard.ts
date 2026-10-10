import { Injectable, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';

export const roleGuard: (allowedRole: string) => CanActivateFn = (allowedRole: string): CanActivateFn => {
  const guard: CanActivateFn = (): boolean => {
    const authState: AuthenticationStateService = inject(AuthenticationStateService);
    const router: Router = inject(Router);

    const role: string | null = authState.role();
    if (role === null) {
      router.navigate(['/auth/login']);
      return false;
    }

    if (role !== allowedRole) {
      router.navigate(['/access-denied']);
      return false;
    }

    return true;
  };
  return guard;
};
