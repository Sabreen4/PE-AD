import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { EvaluationService } from './EvaluationService';
import { Attendance } from '../models/attendance.model';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private readonly svc = inject(EvaluationService);

  list(params?: Record<string, string>): Observable<{ items: Attendance[] }> {
    return this.svc.getEnrollmentAttendance(0, 1, 100).pipe(
      map((dto) => ({
        items: (dto?.data ?? []) as {} as Attendance[],
      }))
    );
  }
}
