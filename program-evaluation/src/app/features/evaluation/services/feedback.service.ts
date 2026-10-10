import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { EvaluationService } from './EvaluationService';
import { Feedback } from '../models/feedback.model';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly svc = inject(EvaluationService);

  list(): Observable<{ items: Feedback[] }> {
    return this.svc.getFeedbackByWeeklyEvaluation(0, 1, 100).pipe(
      map((dto) => ({
        items: (dto?.data ?? []) as {} as Feedback[],
      }))
    );
  }
}
