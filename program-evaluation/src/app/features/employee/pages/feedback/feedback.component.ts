import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ProgramService } from '@features/program/services/ProgramService';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { ProgramDtoPagedResponseDto } from '@features/program/models/ProgramDtoPagedResponseDto';
import { ProgramWeekDto } from '@features/program/models/ProgramWeekDto';
import { Program } from '@features/program/models/program.model';
import { ProgramWeekDtoPagedResponseDto } from '@features/program/models/ProgramWeekDtoPagedResponseDto';
import { FeedbackDto } from '@features/evaluation/models/FeedbackDto';
import { FeedbackListResponseDto } from '@features/evaluation/models/FeedbackListResponseDto';
import { WeeklyEvaluationListResponseDto } from '@features/evaluation/models/WeeklyEvaluationListResponseDto';
import { DropdownModule } from 'primeng/dropdown';

@Component({
  selector: 'app-employee-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule, BackNavComponent, DropdownModule],
  templateUrl: './feedback.component.html',
  styleUrl: './feedback.component.scss',
})
export class EmployeeFeedbackComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly evalService: EvaluationService = inject(EvaluationService);

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<ProgramWeekDto[]>([]);
  public readonly feedbackRecords = signal<FeedbackDto[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly selectedWeekId = signal<string>('');

  public readonly formattedWeeks = computed(() => {
    return [
      { id: '', label: 'All Weeks' },
      ...this.weeks().map((w) => ({
        id: String(w.id ?? ''),
        label: `Week ${w.weekNumber}`,
      })),
    ];
  });
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly isForbidden = signal(false);

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.weeks.set([]);
    this.feedbackRecords.set([]);
    this.selectedWeekId.set('');
    this.errorMessage.set(null);
    this.isForbidden.set(false);
    if (programId) this.loadWeeks(programId);
  }

  public onWeekChange(weekId: string): void {
    this.selectedWeekId.set(weekId);
    this.feedbackRecords.set([]);
    this.errorMessage.set(null);
    this.isForbidden.set(false);
    if (weekId) this.loadFeedback();
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.programService.getProgram(1, 100).subscribe({
      next: (res: ProgramDtoPagedResponseDto) => { this.programs.set((res?.data ?? []) as {} as Program[]); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadWeeks(programId: string): void {
    this.isLoading.set(true);
    this.programService.getProgramProgramWeek(Number(programId)).subscribe({
      next: (res: ProgramWeekDtoPagedResponseDto) => { this.weeks.set(res?.data ?? []); this.isLoading.set(false); },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view program weeks.'); }
        else { this.errorMessage.set('Failed to load weeks.'); }
      },
    });
  }

  private loadFeedback(): void {
    const programId = this.selectedProgramId();
    const weekId = this.selectedWeekId();
    if (!programId || !weekId) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.isForbidden.set(false);

    console.log('Selected Program:', programId);
    console.log('Selected ProgramWeek:', weekId);

    this.enrollmentService.getMyEnrollment(programId).subscribe({
      next: (enrollment) => {
        if (!enrollment?.id) {
          console.log('No enrollment found for program', programId);
          this.isLoading.set(false);
          this.errorMessage.set('You are not enrolled in this program.');
          return;
        }

        const enrollmentId = enrollment.id;
        console.log('EnrollmentId:', enrollmentId);

        this.evalService.getEnrollmentWeeklyEvaluation(enrollmentId, 1, 100).subscribe({
          next: (evalRes: WeeklyEvaluationListResponseDto) => {
            const weeklyEvals = evalRes?.data ?? [];
            const match = weeklyEvals.find((e) => e.programWeekId === Number(weekId));

            if (!match?.id) {
              console.log('No weekly evaluation found for programWeekId', weekId);
              this.isLoading.set(false);
              this.errorMessage.set('No feedback records found for this week.');
              return;
            }

            const weeklyEvaluationId = match.id;
            console.log('WeeklyEvaluationId:', weeklyEvaluationId);

            this.evalService.getFeedbackByWeeklyEvaluation(weeklyEvaluationId, 1, 50).subscribe({
              next: (res: FeedbackListResponseDto) => {
                const allFeedback = res?.data ?? [];
                console.log('Feedback Response:', res);
                this.isLoading.set(false);
                this.feedbackRecords.set(allFeedback);
                if (allFeedback.length === 0) {
                  this.errorMessage.set('No feedback records found for this week.');
                }
              },
              error: (err: HttpErrorResponse) => {
                this.isLoading.set(false);
                if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view feedback records. This endpoint may require backend authorization for employee self-access.'); }
                else { this.errorMessage.set('Failed to load feedback.'); }
              },
            });
          },
          error: (err: HttpErrorResponse) => {
            this.isLoading.set(false);
            if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view weekly evaluations.'); }
            else { this.errorMessage.set('Failed to load weekly evaluations.'); }
          },
        });
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('Failed to load your enrollment.');
      },
    });
  }

  public feedbackTypeClass(typeName: string | null | undefined): string {
    if (typeName?.toLowerCase().includes('green') || typeName?.toLowerCase().includes('positive')) return 'positive';
    if (typeName?.toLowerCase().includes('red') || typeName?.toLowerCase().includes('negative')) return 'negative';
    return 'neutral';
  }
}
