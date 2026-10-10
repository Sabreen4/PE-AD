import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { apiConfig } from '@core/config/api.config';
import { IDENTITY_ENDPOINTS } from '@core/api/identity/identity-endpoints';
import { LoginRequest } from '@core/models/identity/login-request.model';
import { LoginResponse } from '@core/models/identity/login-response.model';
import { VerifyOtpRequest } from '@core/models/identity/verify-otp-request.model';
import { TokenResponse } from '@core/models/identity/token-response.model';
import { RefreshTokenRequest } from '@core/models/identity/refresh-token-request.model';
import { LogoutRequest } from '@core/models/identity/logout-request.model';
import { CreateOrganizationRequest } from '@core/models/identity/create-organization-request.model';
import { CreateUserRequest } from '@core/models/identity/create-user-request.model';
import { UserResponse } from '@core/models/identity/user-response.model';
import { OrganizationResponse } from '@core/models/identity/organization-response.model';
import { IdResponse } from '@core/models/identity/id-response.model';

@Injectable({ providedIn: 'root' })
export class IdentityApiService {
  private readonly http: HttpClient = inject(HttpClient);
  private readonly baseUrl: string = apiConfig.gatewayUrl;

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.login}`, request);
  }

  verifyOtp(request: VerifyOtpRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.verifyOtp}`, request);
  }

  refreshToken(request: RefreshTokenRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.refreshToken}`, request);
  }

  logout(request: LogoutRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}${IDENTITY_ENDPOINTS.logout}`, request);
  }

  createOrganization(request: CreateOrganizationRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.organizations}`, request);
  }

  getOrganizations(): Observable<OrganizationResponse[]> {
    return this.http.get<OrganizationResponse[]>(`${this.baseUrl}${IDENTITY_ENDPOINTS.organizations}`);
  }

  getOrganizationById(id: number): Observable<OrganizationResponse> {
    return this.http.get<OrganizationResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.organizationById(id)}`);
  }

  getUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(`${this.baseUrl}${IDENTITY_ENDPOINTS.users}`);
  }

  getUserById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.userById(id)}`);
  }

  createUser(request: CreateUserRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`${this.baseUrl}${IDENTITY_ENDPOINTS.users}`, request);
  }
}
