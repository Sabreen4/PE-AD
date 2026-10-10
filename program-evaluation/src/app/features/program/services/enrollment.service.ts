import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, forkJoin, of, catchError } from 'rxjs';
import { ProgramService as MonolithProgramService } from './ProgramService';
import { Enrollment } from '../models/enrollment.model';
import { EnrollmentDetailDto } from '../models/EnrollmentDetailDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';


@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private readonly http = inject(HttpClient);
  private readonly svc = inject(MonolithProgramService);

  listByProgram(
    programId: string,
    params?: Record<string, string | number | boolean | null>
  ): Observable<{ items: Enrollment[]; total?: number }> {
    const page: number = (params?.['page'] as number) ?? 1;
    const limit: number = (params?.['limit'] as number) ?? 100;
    const search = params?.['search'] as string | undefined;
    const queryParams: Record<string, string | number> = { pageNumber: page, pageSize: limit };
    if (search) queryParams['search'] = search;
    return this.http.get<{ data?: Enrollment[]; pagination?: { totalRecords?: number } }>(
      `${OpenAPI.BASE}/program/${Number(programId)}/enrollment`,
      { params: queryParams }
    ).pipe(
      map((res) => ({
        items: (res?.data ?? []) as {} as Enrollment[],
        total: res?.pagination?.totalRecords,
      }))
    );
  }

  enroll(programId: string, employeeIds: string[]): Observable<void> {
    if (employeeIds.length === 0) return of(void 0);
    const calls: Observable<Record<string, string | number | boolean | null>>[] = employeeIds.map((id) =>
      this.svc.postProgramEnrollment(Number(programId), id)
    );
    return forkJoin(calls).pipe(map(() => void 0));
  }

  getMyEnrollment(programId: string): Observable<EnrollmentDetailDto | null> {
    return this.svc.getProgramEnrollmentMy(Number(programId)).pipe(
      catchError(() => of(null))
    );
  }

  unenroll(programId: string, enrollmentId: string): Observable<void> {
    return this.svc.deleteProgramEnrollment(Number(programId), Number(enrollmentId)).pipe(
      map(() => void 0)
    );
  }
}
