import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { WeeklyEvaluationService } from '@features/evaluation/services/weekly-evaluation.service';
import { WeeklyEvaluation } from '@features/evaluation/models/weekly-evaluation.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-weekly-evaluations',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, LoadingSpinnerComponent, EmptyStateComponent, BackNavComponent],
  templateUrl: './weekly-evaluations.component.html',
  styleUrls: ['./weekly-evaluations.component.scss'],
})
export class WeeklyEvaluationsComponent implements OnInit {
  private weeklyEvaluationService = inject(WeeklyEvaluationService);

  loading = signal(false);
  evaluations = signal<WeeklyEvaluation[]>([]);
  displayedColumns = ['employeeName', 'weekStart', 'weekEnd', 'score', 'evaluatedBy'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.weeklyEvaluationService.list().subscribe({
      next: (result) => {
        this.evaluations.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
