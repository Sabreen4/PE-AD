import { Injectable } from '@angular/core';

export interface DecodedToken {
  userId: string;
  email: string;
  role: string;
  organizationId: number;
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly accessTokenKey: string = 'access_token';
  private readonly refreshTokenKey: string = 'refresh_token';

  public getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  public setAccessToken(token: string): void {
    localStorage.setItem(this.accessTokenKey, token);
  }

  public getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  public setRefreshToken(token: string): void {
    localStorage.setItem(this.refreshTokenKey, token);
  }

  public clear(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
  }

  public decodeToken(token: string): DecodedToken | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        userId: String(
          payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? ''
        ),
        email: String(payload['email'] ?? payload['sub'] ?? ''),
        role: String(
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? ''
        ),
        organizationId: Number(payload['organizationId'] ?? 0),
        exp: Number(payload['exp'] ?? 0),
      };
    } catch {
      return null;
    }
  }

  public isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded || decoded.exp === 0) return false;
    return decoded.exp * 1000 < Date.now();
  }

  public hasValidToken(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    return !this.isTokenExpired(token);
  }
}
