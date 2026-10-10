import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { LoginRequestDto } from '../models/LoginRequestDto';
import type { LogoutRequestDto } from '../models/LogoutRequestDto';
import type { RefreshTokenRequestDto } from '../models/RefreshTokenRequestDto';
import type { VerifyOtpRequestDto } from '../models/VerifyOtpRequestDto';
import type { CreateOrganizationRequestDto } from '../models/CreateOrganizationRequestDto';
import type { CreateUserRequestDto } from '../models/CreateUserRequestDto';
import type { UpdateUserRequestDto } from '../models/UpdateUserRequestDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { request as __request } from '@core/api-gen/request';

@Injectable({
    providedIn: 'root',
})
export class IdentityService {
    constructor(public readonly http: HttpClient) {}

    // ---- Auth ----

    public postAuthLogin(requestBody?: LoginRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/auth/login', body: requestBody, mediaType: 'application/json',
        });
    }

    public postAuthVerifyOtp(requestBody?: VerifyOtpRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/auth/verify-otp', body: requestBody, mediaType: 'application/json',
        });
    }

    public postAuthRefreshToken(requestBody?: RefreshTokenRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/auth/refresh-token', body: requestBody, mediaType: 'application/json',
        });
    }

    public postAuthLogout(requestBody?: LogoutRequestDto): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/auth/logout', body: requestBody, mediaType: 'application/json',
        });
    }

    // ---- Organization ----

    public getOrganization(): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/organization',
        });
    }

    public postOrganization(requestBody?: CreateOrganizationRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/organization', body: requestBody, mediaType: 'application/json',
        });
    }

    public getOrganization1(id: number): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/organization/{id}', path: { id },
        });
    }

    public putOrganization(id: number, requestBody?: CreateOrganizationRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/organization/{id}', path: { id }, body: requestBody, mediaType: 'application/json',
        });
    }

    public deleteOrganization(id: number): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE', url: '/organization/{id}', path: { id },
        });
    }

    // ---- User ----

    public getUser(): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/user',
        });
    }

    public postUser(requestBody?: CreateUserRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/user', body: requestBody, mediaType: 'application/json',
        });
    }

    public getUser1(id: string): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/user/{id}', path: { id },
        });
    }

    public putUser(id: string, requestBody?: UpdateUserRequestDto): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/user/{id}', path: { id }, body: requestBody, mediaType: 'application/json',
        });
    }

    public deleteUser(id: string): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE', url: '/user/{id}', path: { id },
        });
    }
}
