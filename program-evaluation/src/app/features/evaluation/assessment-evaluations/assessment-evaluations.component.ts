import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { AssessmentEvaluationService } from '@features/evaluation/services/assessment-evaluation.service';
import { AssessmentEvaluation } from '@features/evaluation/models/assessment-evaluation.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-assessment-evaluations',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, LoadingSpinnerComponent, EmptyStateComponent, BackNavComponent],
  templateUrl: './assessment-evaluations.component.html',
  styleUrls: ['./assessment-evaluations.component.scss'],
})
export class AssessmentEvaluationsComponent implements OnInit {
  private assessmentEvaluationService = inject(AssessmentEvaluationService);

  loading = signal(false);
  evaluations = signal<AssessmentEvaluation[]>([]);
  displayedColumns = ['employeeName', 'assessmentType', 'score', 'conductedAt', 'evaluatedBy'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.assessmentEvaluationService.list().subscribe({
      next: (result) => {
        this.evaluations.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
