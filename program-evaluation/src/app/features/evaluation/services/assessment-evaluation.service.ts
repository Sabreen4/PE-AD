import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { EvaluationService } from './EvaluationService';
import { AssessmentEvaluation } from '../models/assessment-evaluation.model';

@Injectable({ providedIn: 'root' })
export class AssessmentEvaluationService {
  private readonly svc = inject(EvaluationService);

  list(): Observable<{ items: AssessmentEvaluation[] }> {
    return this.svc.getEmployeeAssignmentAssessmentEvaluation(0).pipe(
      map((dto) => ({
        items: dto ? [dto as {} as AssessmentEvaluation] : [],
      }))
    );
  }
}
