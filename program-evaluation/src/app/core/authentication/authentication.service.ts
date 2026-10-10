import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, of, switchMap, tap, throwError } from 'rxjs';
import { IdentityApiService } from '@core/api/identity/identity-api.service';
import { TokenStorageService, DecodedToken } from '@core/authentication/token-storage.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { LoginRequest } from '@core/models/identity/login-request.model';
import { LoginResponse } from '@core/models/identity/login-response.model';
import { VerifyOtpRequest } from '@core/models/identity/verify-otp-request.model';
import { TokenResponse } from '@core/models/identity/token-response.model';
import { AuthenticatedUser } from '@core/models/identity/authenticated-user.model';
import { SignalRService } from '@core/services/signalr.service';
import { NotificationService } from '@core/services/notification.service';

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
  private readonly api: IdentityApiService = inject(IdentityApiService);
  private readonly tokenStorage: TokenStorageService = inject(TokenStorageService);
  private readonly state: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly router: Router = inject(Router);
  private readonly signalr: SignalRService = inject(SignalRService);
  private readonly notificationService: NotificationService = inject(NotificationService);

  public readonly isRefreshing = signal<boolean>(false);

  public login(email: string): Observable<LoginResponse> {
    const request: LoginRequest = { email };
    return this.api.login(request).pipe(
      tap(() => {
        this.state.clearUser();
        this.tokenStorage.clear();
      })
    );
  }

  public verifyOtp(email: string, otp: string): Observable<TokenResponse> {
    const request: VerifyOtpRequest = { email, otp };
    return this.api.verifyOtp(request).pipe(
      tap((response: TokenResponse) => {
        this.tokenStorage.setAccessToken(response.accessToken);
        this.tokenStorage.setRefreshToken(response.refreshToken);
      }),
      switchMap((response: TokenResponse) => {
        const user: AuthenticatedUser = this.decodeUserFromToken(response.accessToken, email);
        this.state.setUser(user);
        const role: string | null = this.state.role();
        this.navigateToRoleDashboard(role);
        this.signalr.start().then(() => this.notificationService.init());
        return of(response);
      })
    );
  }

  public refreshToken(): Observable<TokenResponse> {
    const currentRefreshToken: string | null = this.tokenStorage.getRefreshToken();

    if (currentRefreshToken === null) {
      this.forceLogout();
      return throwError(() => new Error('No refresh token available'));
    }

    this.isRefreshing.set(true);

    return this.api.refreshToken({ refreshToken: currentRefreshToken }).pipe(
      tap((response: TokenResponse) => {
        this.tokenStorage.setAccessToken(response.accessToken);
        this.isRefreshing.set(false);
      }),
      catchError((error: Error) => {
        this.isRefreshing.set(false);
        return throwError(() => error);
      })
    );
  }

  public tryRestoreSession(): boolean {
    const token = this.tokenStorage.getAccessToken();
    if (!token) return false;

    if (this.state.isAuthenticated()) return true;

    const decoded: DecodedToken | null = this.tokenStorage.decodeToken(token);
    if (!decoded || !decoded.userId || !decoded.role) {
      this.state.clearUser();
      this.tokenStorage.clear();
      return false;
    }

    if (this.tokenStorage.isTokenExpired(token)) {
      const refreshToken = this.tokenStorage.getRefreshToken();
      if (!refreshToken) {
        this.state.clearUser();
        this.tokenStorage.clear();
        return false;
      }
      return false;
    }

    const user: AuthenticatedUser = {
      userId: decoded.userId,
      email: decoded.email,
      role: decoded.role,
      organizationId: decoded.organizationId,
    };
    this.state.setUser(user);
    return true;
  }

  public logout(): void {
    this.signalr.stop();
    const refreshToken: string | null = this.tokenStorage.getRefreshToken();
    if (refreshToken !== null) {
      this.api.logout({ refreshToken }).subscribe({
        error: () => this.forceLogout(),
        complete: () => this.forceLogout(),
      });
    } else {
      this.forceLogout();
    }
  }

  public getAccessToken(): string | null {
    return this.tokenStorage.getAccessToken();
  }

  public isAuthenticated(): boolean {
    return this.state.isAuthenticated();
  }

  private forceLogout(): void {
    this.state.clearUser();
    this.tokenStorage.clear();
    this.router.navigate(['/auth/login']);
  }

  private decodeUserFromToken(accessToken: string, email: string): AuthenticatedUser {
    const decoded: DecodedToken | null = this.tokenStorage.decodeToken(accessToken);
    if (decoded) {
      return {
        userId: decoded.userId,
        email: email || decoded.email,
        role: decoded.role,
        organizationId: decoded.organizationId,
      };
    }
    return { userId: '', email, role: '', organizationId: 0 };
  }

  private navigateToRoleDashboard(role: string | null): void {
    switch (role) {
      case 'Admin':
        this.router.navigate(['/admin']);
        break;
      case 'Employee':
        this.router.navigate(['/employee']);
        break;
      case 'Evaluator':
        this.router.navigate(['/evaluator']);
        break;
      default:
        this.router.navigate(['/dashboard']);
        break;
    }
  }
}
