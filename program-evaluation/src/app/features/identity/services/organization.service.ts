import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { IdentityService } from './IdentityService';
import { Organization } from '../models/organization.model';

@Injectable({ providedIn: 'root' })
export class OrganizationService {
  private readonly svc = inject(IdentityService);

  list(params?: Record<string, string | number | boolean | null>): Observable<{ items: Organization[] }> {
    return this.svc.getOrganization().pipe(
      map((res) => ({
        items: ((res as { data?: Record<string, string | number | boolean | null>[] })?.data ?? []) as {} as Organization[],
      }))
    );
  }

  getById(id: string): Observable<Organization> {
    return this.svc.getOrganization1(Number(id)).pipe(
      map((res) => res as {} as Organization)
    );
  }

  create(value: Record<string, string | number | boolean | null>): Observable<Organization> {
    return this.svc.postOrganization(value).pipe(
      map((res) => res as {} as Organization)
    );
  }

  update(id: string, value: Record<string, string | number | boolean | null>): Observable<Organization> {
    return this.svc.putOrganization(Number(id), value).pipe(
      map((res) => res as {} as Organization)
    );
  }

  deactivate(id: string): Observable<void> {
    return this.svc.deleteOrganization(Number(id)).pipe(
      map(() => void 0)
    );
  }
}
