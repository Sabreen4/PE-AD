import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { DropdownModule } from 'primeng/dropdown';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { ToastService } from '@shared/toast/toast.service';

interface EvaluationDisplay {
  programWeekId: number;
  attendancePercentage: number;
  attendanceScore: number;
  engagementScore: number;
  applicationScore: number;
  weeklyAverage: number;
  status: string;
  lastUpdated: string | null;
}

@Component({
  selector: 'app-employee-weekly-score',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, BackNavComponent, DropdownModule],
  templateUrl: './weekly-score.component.html',
  styleUrl: './weekly-score.component.scss'
})
export class EmployeeWeeklyScoreComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly evalService: EvaluationService = inject(EvaluationService);
  private readonly toast: ToastService = inject(ToastService);

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly selectedWeekId = signal<string>('');

  public readonly formattedWeeks = computed(() => {
    return [
      { id: '', label: 'Select a week' },
      ...this.weeks().map((w) => ({
        id: w.id,
        label: `Week ${w.weekNumber}${w.topic ? ': ' + w.topic : ''}`,
      })),
    ];
  });
  public readonly isLoading = signal(false);
  public readonly evaluation = signal<EvaluationDisplay | null>(null);

  public readonly selectedProgramName = computed(() => {
    const id = this.selectedProgramId();
    if (!id) return '';
    return this.programs().find((p) => p.id === id)?.name ?? '';
  });

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.selectedWeekId.set('');
    this.weeks.set([]);
    this.evaluation.set(null);
    if (programId) this.loadWeeks(programId);
  }

  public onWeekChange(weekId: string): void {
    this.selectedWeekId.set(weekId);
    this.evaluation.set(null);
    if (weekId && this.selectedProgramId()) this.loadEvaluation(this.selectedProgramId(), weekId);
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

  private loadEvaluation(programId: string, weekId: string): void {
    this.isLoading.set(true);
    this.enrollmentService.getMyEnrollment(programId).subscribe({
      next: (enrollment) => {
        if (!enrollment) {
          this.toast.warning('Weekly Score', 'You are not enrolled in this program.');
          this.isLoading.set(false);
          return;
        }
        this.evalService.getEnrollmentWeeklyEvaluation(Number(enrollment.id), 1, 100).subscribe({
          next: (res) => {
            const evals = res?.data ?? [];
            const match = evals.find((e) => e.programWeekId === Number(weekId));
            if (match) {
              this.evaluation.set({
                programWeekId: match.programWeekId ?? 0,
                attendancePercentage: (match as {} as { attendancePercentage?: number }).attendancePercentage ?? 0,
                attendanceScore: match.attendanceScore ?? 0,
                engagementScore: match.engagementScore ?? 0,
                applicationScore: match.applicationScore ?? 0,
                weeklyAverage: match.weeklyAverage ?? 0,
                status: 'Completed',
                lastUpdated: (match as {} as { updatedAt?: string | null }).updatedAt ?? null,
              });
            } else {
              this.evaluation.set(null);
              this.toast.info('Weekly Score', 'No evaluation found for this week.');
            }
            this.isLoading.set(false);
          },
          error: () => { this.isLoading.set(false); this.toast.error('Weekly Score', 'Failed to load evaluation.'); },
        });
      },
      error: () => { this.isLoading.set(false); this.toast.error('Weekly Score', 'Failed to load enrollment.'); },
    });
  }
}
