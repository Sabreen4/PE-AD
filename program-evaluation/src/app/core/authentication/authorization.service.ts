import { Injectable, inject } from '@angular/core';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { UserRole } from '@core/models/identity/user-role.enum';

@Injectable({ providedIn: 'root' })
export class AuthorizationService {
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);

  public hasRole(requiredRole: string): boolean {
    const currentRole: string | null = this.authState.role();
    if (currentRole === null) {
      return false;
    }
    return currentRole === requiredRole;
  }

  public isAdmin(): boolean {
    return this.authState.isAdmin();
  }

  public isEmployee(): boolean {
    return this.authState.isEmployee();
  }

  public isEvaluator(): boolean {
    return this.authState.isEvaluator();
  }

  public isAuthorizedForRoute(routeRole: string): boolean {
    if (routeRole === UserRole.Admin) {
      return this.isAdmin();
    }
    if (routeRole === UserRole.Employee) {
      return this.isEmployee();
    }
    if (routeRole === UserRole.Evaluator) {
      return this.isEvaluator();
    }
    return false;
  }
}
