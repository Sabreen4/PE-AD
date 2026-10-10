import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { EvaluationService } from './EvaluationService';
import { WeeklyEvaluation } from '../models/weekly-evaluation.model';

@Injectable({ providedIn: 'root' })
export class WeeklyEvaluationService {
  private readonly svc = inject(EvaluationService);

  list(): Observable<{ items: WeeklyEvaluation[] }> {
    return this.svc.getEnrollmentWeeklyEvaluation(0, 1, 100).pipe(
      map((dto) => ({
        items: (dto?.data ?? []) as {} as WeeklyEvaluation[],
      }))
    );
  }
}
