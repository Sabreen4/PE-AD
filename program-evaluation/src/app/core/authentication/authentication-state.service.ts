import { Injectable, signal, computed } from '@angular/core';
import { AuthenticatedUser } from '@core/models/identity/authenticated-user.model';
import { UserRole } from '@core/models/identity/user-role.enum';

@Injectable({ providedIn: 'root' })
export class AuthenticationStateService {
  private readonly userSignal = signal<AuthenticatedUser | null>(this.loadUser());

  public readonly user = this.userSignal.asReadonly();

  public readonly isAuthenticated = computed<boolean>(() => this.userSignal() !== null);

  public readonly role = computed<string | null>(() => this.userSignal()?.role ?? null);

  public readonly isAdmin = computed<boolean>(() => this.role() === UserRole.Admin);

  public readonly isEmployee = computed<boolean>(() => this.role() === UserRole.Employee);

  public readonly isEvaluator = computed<boolean>(() => this.role() === UserRole.Evaluator);

  private readonly userStorageKey: string = 'authenticated_user';

  public setUser(user: AuthenticatedUser): void {
    this.userSignal.set(user);
    localStorage.setItem(this.userStorageKey, JSON.stringify(user));
  }

  public clearUser(): void {
    this.userSignal.set(null);
    localStorage.removeItem(this.userStorageKey);
  }

  private loadUser(): AuthenticatedUser | null {
    const stored: string | null = localStorage.getItem(this.userStorageKey);
    if (stored === null) {
      return null;
    }
    try {
      const parsed: AuthenticatedUser = JSON.parse(stored) as AuthenticatedUser;
      if (parsed && typeof parsed.userId === 'string' && typeof parsed.role === 'string') {
        return parsed;
      }
      return null;
    } catch {
      return null;
    }
  }
}
