import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { FeedbackService } from '@features/evaluation/services/feedback.service';
import { Feedback } from '@features/evaluation/models/feedback.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-qualitative-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatCardModule, LoadingSpinnerComponent, EmptyStateComponent, BackNavComponent],
  templateUrl: './qualitative-feedback.component.html',
  styleUrls: ['./qualitative-feedback.component.scss'],
})
export class QualitativeFeedbackComponent implements OnInit {
  private feedbackService = inject(FeedbackService);

  loading = signal(false);
  feedbacks = signal<Feedback[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.feedbackService.list().subscribe({
      next: (result) => {
        this.feedbacks.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
