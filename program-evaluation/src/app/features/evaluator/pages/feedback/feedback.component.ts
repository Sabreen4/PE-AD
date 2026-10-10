import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { CreateFeedbackDto } from '@features/evaluation/models/CreateFeedbackDto';
import { FeedbackDto } from '@features/evaluation/models/FeedbackDto';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { Enrollment } from '@features/program/models/enrollment.model';
import { HttpErrorResponse } from '@angular/common/http';
import { FEEDBACK_TYPE_OPTIONS } from '@features/evaluation/models/feedback-type.constants';
import { ToastService } from '@shared/toast/toast.service';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { DropdownModule } from 'primeng/dropdown';

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule, BackNavComponent, DropdownModule],
  templateUrl: './feedback.component.html',
  styleUrl: './feedback.component.scss'
})
export class FeedbackComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly evalService: EvaluationService = inject(EvaluationService);
  private readonly toast: ToastService = inject(ToastService);

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly enrollments = signal<Enrollment[]>([]);
  public readonly feedbackTypes = signal(FEEDBACK_TYPE_OPTIONS);

  public readonly formattedWeeks = computed(() => {
    return [
      { id: '', label: 'Select a week' },
      ...this.weeks().map((w) => ({
        id: w.id,
        label: `Week ${w.weekNumber}${w.topic ? ': ' + w.topic : ''}`,
      })),
    ];
  });

  public readonly formattedEnrollments = computed(() => {
    return this.enrollments().map((enr) => ({
      id: Number(enr.id),
      name: enr.employeeName,
    }));
  });
  public readonly selectedProgramId = signal<string>('');
  public readonly selectedWeekId = signal<string>('');
  public readonly selectedAssignmentId = signal<number | null>(null);
  public readonly selectedEmployeeId = signal<string>('');
  public readonly selectedFeedbackType = signal<string>('');
  protected readonly Number = Number;
  public readonly commentText = signal<string>('');
  public readonly weeklyEvaluationId = signal<number | null>(null);
  public readonly feedbackRecords = signal<FeedbackDto[]>([]);
  public readonly isLoading = signal(false);
  public readonly isSubmitting = signal(false);

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.selectedWeekId.set('');
    this.selectedAssignmentId.set(null);
    this.selectedEmployeeId.set('');
    this.weeklyEvaluationId.set(null);
    this.weeks.set([]);
    this.enrollments.set([]);
    if (programId) {
      this.loadWeeks(programId);
      this.loadEnrollments(programId);
    }
  }

  public onWeekChange(weekId: string): void {
    this.selectedWeekId.set(weekId);
    this.selectedAssignmentId.set(null);
    this.selectedEmployeeId.set('');
    this.weeklyEvaluationId.set(null);
  }

  public onEmployeeChange(assignmentId: number | null): void {
    this.selectedAssignmentId.set(assignmentId);
    this.selectedEmployeeId.set('');
    this.weeklyEvaluationId.set(null);
    this.feedbackRecords.set([]);
    if (assignmentId && this.selectedWeekId() && this.selectedProgramId()) {
      const enrollment = this.enrollments().find((e) => Number(e.id) === assignmentId);
      if (enrollment) { this.selectedEmployeeId.set(enrollment.employeeId); }
      this.lookupWeeklyEvaluation(assignmentId);
    }
  }

  public submitFeedback(): void {
    if (!this.selectedFeedbackType()) { this.toast.warning('Feedback', 'Please select a feedback type.'); return; }
    const assignmentId = this.selectedAssignmentId();
    const weeklyEvalId = this.weeklyEvaluationId();
    if (!weeklyEvalId) { this.toast.warning('Feedback', 'No weekly evaluation found. Please create a weekly evaluation first.'); return; }
    this.isSubmitting.set(true);

    const dto: CreateFeedbackDto = {
      employeeId: this.selectedEmployeeId() || undefined,
      employeeAssignmentId: assignmentId ?? undefined,
      weeklyEvaluationId: weeklyEvalId,
      feedbackTypeCode: this.selectedFeedbackType(),
      comment: this.commentText().trim() || null,
    };
    this.evalService.postFeedback(dto).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toast.success('Feedback', 'Feedback submitted successfully.');
        this.commentText.set('');
        this.selectedFeedbackType.set('');
        this.selectedEmployeeId.set('');
        this.loadFeedbackRecords(weeklyEvalId);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.toast.error('Feedback', err.error?.message || 'Failed to submit feedback.');
      },
    });
  }

  private lookupWeeklyEvaluation(assignmentId: number): void {
    this.evalService.getEnrollmentWeeklyEvaluation(assignmentId, 1, 100).subscribe({
      next: (res) => {
        const evals = res?.data ?? [];
        const match = evals.find((e) => e.programWeekId === Number(this.selectedWeekId()));
        if (match?.id) {
          this.weeklyEvaluationId.set(match.id);
          this.loadFeedbackRecords(match.id);
        }
      },
    });
  }

  private loadFeedbackRecords(weeklyEvalId: number): void {
    this.evalService.getFeedbackByWeeklyEvaluation(weeklyEvalId, 1, 100).subscribe({
      next: (res) => {
        this.feedbackRecords.set(res?.data ?? []);
      },
    });
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => { this.programs.set(res.items); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadWeeks(programId: string): void {
    this.programWeekService.listByProgram(programId, { page: 1, limit: 100 }).subscribe({
      next: (res) => { this.weeks.set(res.items); },
    });
  }

  private loadEnrollments(programId: string): void {
    this.enrollmentService.listByProgram(programId, { page: 1, limit: 100 }).subscribe({
      next: (res) => { this.enrollments.set(res.items); },
    });
  }
}
