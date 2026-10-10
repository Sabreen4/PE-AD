import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { IdentityService } from './IdentityService';
import { User } from '../models/user.model';
import type { CreateUserRequestDto } from '../models/CreateUserRequestDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { PaginatedResponse } from '@core/models/paginated-response.model';

export function toUser(raw: Record<string, any>): User {
  const roles: string[] = Array.isArray(raw['roles'])
    ? (raw['roles'] as string[])
    : typeof raw['role'] === 'string'
      ? [(raw['role'] as string)]
      : [];

  let firstName = String(raw['firstName'] ?? raw['first_name'] ?? '');
  let lastName = String(raw['lastName'] ?? raw['last_name'] ?? '');

  if (!firstName && raw['name']) {
    const parts = String(raw['name']).split(' ');
    firstName = parts[0] || '';
    lastName = parts.slice(1).join(' ') || '';
  }

  const email = String(raw['email'] ?? raw['emailAddress'] ?? raw['userName'] ?? '');
  const phone = raw['phone'] ?? raw['phoneNumber'] ?? null;

  return {
    id: String(raw['id'] ?? ''),
    email,
    firstName,
    lastName,
    isActive: raw['isActive'] !== false,
    organizationId: String(raw['organizationId'] ?? ''),
    organizationName: String(raw['organizationName'] ?? ''),
    roles,
    role: String(raw['role'] ?? (roles.length > 0 ? roles[0] : '')),
    phone: phone ? String(phone) : null,
    createdAt: String(raw['createdAt'] ?? ''),
    updatedAt: String(raw['updatedAt'] ?? ''),
  };
}

function normalizeUsers(response: Partial<User>[] | { items?: Partial<User>[]; data?: Partial<User>[]; total?: number; page?: number; limit?: number }): PaginatedResponse<User> {
  const items: Partial<User>[] = Array.isArray(response)
    ? response
    : (response['items'] as Partial<User>[]) ??
      (response['data'] as Partial<User>[]) ??
      [];
  return {
    items: items.map(toUser),
    total: !Array.isArray(response)
      ? response['total'] as number | undefined
      : undefined,
    page: !Array.isArray(response)
      ? response['page'] as number | undefined
      : undefined,
    limit: !Array.isArray(response)
      ? response['limit'] as number | undefined
      : undefined,
  };
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly svc = inject(IdentityService);

  list(params?: Record<string, string | number | boolean | null>): Observable<PaginatedResponse<User>> {
    return this.http.get<Partial<User>[] | { items?: Partial<User>[]; data?: Partial<User>[]; total?: number; page?: number; limit?: number }>(
      `${OpenAPI.BASE}/user`,
      { params: params as Record<string, string | number | boolean> }
    ).pipe(map(normalizeUsers));
  }

  getById(id: string): Observable<User> {
    return this.svc.getUser1(id).pipe(
      map((res) => res as {} as User)
    );
  }

  create(value: Partial<User>): Observable<User> {
    return this.svc.postUser(value as {} as CreateUserRequestDto).pipe(
      map((res) => res as {} as User)
    );
  }

  update(id: string, value: Partial<User>): Observable<User> {
    return this.svc.putUser(id, value).pipe(
      map((res) => res as {} as User)
    );
  }

  deactivate(id: string): Observable<void> {
    return this.svc.deleteUser(id).pipe(
      map(() => void 0)
    );
  }
}
