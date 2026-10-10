import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Employee } from '../models/employee.model';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { PaginatedResponse } from '@core/models/paginated-response.model';

function toEmployee(raw: Partial<Employee> & Record<string, string | number | boolean | null | string[] | undefined>): Employee {
  return {
    id: raw['id'] as string,
    userId: raw['userId'] as string ?? raw['id'] as string,
    employeeCode: raw['employeeCode'] as string ?? '',
    firstName: raw['firstName'] as string,
    lastName: raw['lastName'] as string,
    email: raw['email'] as string,
    phone: (raw['phone'] as string | null) ?? null,
    organizationId: String(raw['organizationId'] ?? raw['orgId'] ?? ''),
    organizationName: (raw['organizationName'] ?? '') as string,
    isActive: raw['isActive'] as boolean ?? true,
    enrolledProgramIds: (raw['enrolledProgramIds'] as string[]) ?? [],
    createdAt: (raw['createdAt'] ?? '') as string,
    updatedAt: (raw['updatedAt'] ?? '') as string,
  };
}

function normalizeEmployees(response: Partial<Employee>[] | Record<string, string | number | boolean | null | Partial<Employee>[] | undefined>): PaginatedResponse<Employee> {
  const items: Partial<Employee>[] = Array.isArray(response)
    ? response
    : (response['items'] as Partial<Employee>[]) ??
      (response['data'] as Partial<Employee>[]) ??
      [];
  return {
    items: items.map(toEmployee),
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
export class EmployeeService {
  private readonly http = inject(HttpClient);

  list(params?: Record<string, string | number | boolean | null>): Observable<PaginatedResponse<Employee>> {
    return this.http.get<Partial<Employee>[] | Record<string, string | number | boolean | null | Partial<Employee>[] | undefined>>(
      `${OpenAPI.BASE}/employee`,
      { params: params as Record<string, string | number | boolean> }
    ).pipe(map(normalizeEmployees));
  }

  getByOrganization(
    organizationId: string,
    params?: Record<string, string | number | boolean | null>
  ): Observable<PaginatedResponse<Employee>> {
    return this.http.get<Partial<Employee>[] | Record<string, string | number | boolean | null | Partial<Employee>[] | undefined>>(
      `${OpenAPI.BASE}/organization/${organizationId}/employee`,
      { params: params as Record<string, string | number | boolean> }
    ).pipe(map(normalizeEmployees));
  }

  create(value: Partial<Employee>): Observable<Employee> {
    return this.http.post<Employee>(`${OpenAPI.BASE}/employee`, value);
  }

  update(id: string, value: Partial<Employee>): Observable<Employee> {
    return this.http.put<Employee>(`${OpenAPI.BASE}/employee/${id}`, value);
  }

  deactivate(id: string): Observable<void> {
    return this.http.delete<void>(`${OpenAPI.BASE}/employee/${id}`);
  }
}
