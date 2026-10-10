import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { User } from '../models/user.model';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { PaginatedResponse } from '@core/models/paginated-response.model';

function toUser(raw: Partial<User> & Record<string, string | number | boolean | null | string[] | undefined>): User {
  const roles: string[] = Array.isArray(raw['roles'])
    ? (raw['roles'] as string[])
    : typeof raw['role'] === 'string'
      ? [(raw['role'] as string)]
      : [];
  return {
    id: raw['id'] as string,
    email: raw['email'] as string,
    firstName: raw['firstName'] as string,
    lastName: raw['lastName'] as string,
    isActive: raw['isActive'] as boolean ?? true,
    organizationId: String(raw['organizationId'] ?? ''),
    organizationName: (raw['organizationName'] ?? '') as string,
    roles,
    phone: raw['phone'] as string | null ?? null,
    createdAt: (raw['createdAt'] ?? '') as string,
    updatedAt: (raw['updatedAt'] ?? '') as string,
  };
}

@Injectable({ providedIn: 'root' })
export class EvaluatorService {
  private readonly http = inject(HttpClient);

  getByOrganization(
    organizationId: number,
    params?: Record<string, string | number | boolean | null>
  ): Observable<PaginatedResponse<User>> {
    return this.http.get<Partial<User>[] | Record<string, string | number | boolean | null | Partial<User>[] | undefined>>(
      `${OpenAPI.BASE}/user`,
      {
        params: {
          ...params,
          roleType: 'Evaluator',
          organizationId,
        } as Record<string, string | number | boolean>,
      }
    ).pipe(
      map((response) => {
        const items: Partial<User>[] = Array.isArray(response)
          ? response
          : (response['items'] as Partial<User>[]) ??
            (response['data'] as Partial<User>[]) ??
            [];
        const total: number | undefined = !Array.isArray(response)
          ? response['total'] as number | undefined
          : undefined;
        const users: User[] = items.map(toUser);
        return {
          items: users.filter(
            (u: User) =>
              u.roles.includes('Evaluator') &&
              Number(u.organizationId) === organizationId
          ),
          total,
          page: !Array.isArray(response)
            ? response['page'] as number | undefined
            : undefined,
          limit: !Array.isArray(response)
            ? response['limit'] as number | undefined
            : undefined,
        };
      })
    );
  }
}
