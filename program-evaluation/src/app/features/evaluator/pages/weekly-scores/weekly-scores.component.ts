import { Component, DestroyRef, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { CreateWeeklyEvaluationDto } from '@features/evaluation/models/CreateWeeklyEvaluationDto';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { Enrollment } from '@features/program/models/enrollment.model';
import { ApiError } from '@core/api-gen/ApiError';

import { ToastService } from '@shared/toast/toast.service';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { DropdownModule } from 'primeng/dropdown';

interface ScoreRow {
  enrollmentId: number;
  employeeId: string;
  employeeName: string;
  attendanceScore: number;
  engagementScore: number;
  applicationScore: number;
  weeklyAverage: number;
  hasEvaluation: boolean;
  saving: boolean;
}

@Component({
  selector: 'app-weekly-scores',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, PaginationComponent, BackNavComponent, DropdownModule],
  templateUrl: './weekly-scores.component.html',
  styleUrl: './weekly-scores.component.scss'
})
export class WeeklyScoresComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly evalService: EvaluationService = inject(EvaluationService);
  private readonly toast: ToastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly selectedWeekId = signal<string>('');

  public readonly formattedWeeks = computed(() => {
    return [
      { id: '', label: 'Select Week' },
      ...this.weeks().map((w) => ({
        id: w.id,
        label: `Week ${w.weekNumber}${w.topic ? ': ' + w.topic : ''}`,
      })),
    ];
  });
  public readonly isLoading = signal(false);
  public readonly isSaving = signal(false);
  public readonly scoreRows = signal<ScoreRow[]>([]);
  public readonly searchQuery = signal<string>('');
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(25);

  private readonly search$ = new Subject<string>();

  constructor() {
    this.search$.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value: string) => {
      this.searchQuery.set(value);
      this.currentPage.set(1);
      const programId = this.selectedProgramId();
      const weekId = this.selectedWeekId();
      if (programId && weekId) this.loadEvaluationData(programId, weekId, value);
    });
  }

  public readonly selectedProgramName = computed(() => {
    const id = this.selectedProgramId();
    if (!id) return 'Weekly Scoring';
    return this.programs().find((p) => p.id === id)?.name || 'Program';
  });

  public readonly selectedWeek = computed(() => {
    const id = this.selectedWeekId();
    return this.weeks().find((w) => w.id === id) ?? null;
  });

  public readonly hasUnsaved = computed(() =>
    this.scoreRows().some((r) => !r.hasEvaluation && !r.saving)
  );

  public readonly saveCount = computed(() =>
    this.scoreRows().filter((r) => !r.hasEvaluation).length
  );

  public readonly filteredScoreRows = computed(() => {
    let rows = this.scoreRows();
    const query = this.searchQuery().trim().toLowerCase();
    if (query) {
      rows = rows.filter(
        (r) =>
          r.employeeName.toLowerCase().includes(query) ||
          r.employeeId.toLowerCase().includes(query)
      );
    }
    return rows;
  });

  public readonly totalRecords = computed(() => this.filteredScoreRows().length);

  public readonly paginatedScoreRows = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredScoreRows().slice(start, start + this.pageSize());
  });

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }

  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  public onSearchInput(event: Event): void {
    this.search$.next((event.target as HTMLInputElement).value);
  }

  public clearSearch(input: HTMLInputElement): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    input.value = '';
    const programId = this.selectedProgramId();
    const weekId = this.selectedWeekId();
    if (programId && weekId) this.loadEvaluationData(programId, weekId);
  }

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.selectedWeekId.set('');
    this.weeks.set([]);
    this.scoreRows.set([]);
    this.searchQuery.set('');
    if (programId) this.loadWeeks(programId);
  }

  public onWeekChange(weekId: string): void {
    this.selectedWeekId.set(weekId);
    this.searchQuery.set('');
    if (weekId && this.selectedProgramId()) this.loadEvaluationData(this.selectedProgramId(), weekId);
    else this.scoreRows.set([]);
  }

  public setEngagement(enrollmentId: number, value: number): void {
    this.scoreRows.update((rows) =>
      rows.map((r) =>
        r.enrollmentId === enrollmentId && !r.saving
          ? { ...r, engagementScore: value, weeklyAverage: (r.attendanceScore + value + r.applicationScore) / 3 }
          : r
      )
    );
  }

  public setApplication(enrollmentId: number, value: number): void {
    this.scoreRows.update((rows) =>
      rows.map((r) =>
        r.enrollmentId === enrollmentId && !r.saving
          ? { ...r, applicationScore: value, weeklyAverage: (r.attendanceScore + r.engagementScore + value) / 3 }
          : r
      )
    );
  }

  public averageClass(average: number): string {
    if (average >= 2.5) return 'avg-high';
    if (average >= 1.5) return 'avg-mid';
    return 'avg-low';
  }

  public exportCsv(): void {
    const rows = this.scoreRows();
    if (rows.length === 0) return;
    const headers = ['Employee', 'Attendance', 'Engagement', 'Application', 'Weekly Average', 'Status'];
    const lines = rows.map((r) => [
      r.employeeName,
      r.attendanceScore.toString(),
      r.engagementScore.toString(),
      r.applicationScore.toString(),
      r.weeklyAverage.toFixed(1),
      r.hasEvaluation ? 'Saved' : 'Pending',
    ]);
    const csv = [headers.join(','), ...lines.map((l) => l.join(','))].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `weekly-scores-${this.selectedWeekId()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  public saveAll(): void {
    const toSave = this.scoreRows().filter((r) => !r.hasEvaluation && !r.saving);

    if (toSave.length === 0) return;

    this.isSaving.set(true);

    const programWeekId = Number(this.selectedWeekId());
    let completed = 0;
    const total = toSave.length;
    let savedCount = 0;
    let failCount = 0;

    for (const row of toSave) {
      this.scoreRows.update((r) =>
        r.map((x) => x.enrollmentId === row.enrollmentId ? { ...x, saving: true } : x)
      );

      const dto: CreateWeeklyEvaluationDto = {
        programWeekId,
        engagementScore: row.engagementScore,
        applicationScore: row.applicationScore,
      };

      this.evalService.postEnrollmentWeeklyEvaluation(row.enrollmentId, dto).subscribe({
        next: () => {
          completed++;
          savedCount++;
          if (completed === total) this.finishSave(savedCount, failCount);
        },
        error: (err: Error) => {
          this.scoreRows.update((r) =>
            r.map((x) => x.enrollmentId === row.enrollmentId ? { ...x, saving: false } : x)
          );
          completed++;
          failCount++;
          const apiErr = err as ApiError;
          const body = typeof apiErr.body === 'object' && apiErr.body ? (apiErr.body as Record<string, string | number | boolean | null>) : null;
          const detail = body ? String(body['detail'] ?? body['message'] ?? '') : '';
          const msg = detail || apiErr.message || `Failed to save scores for ${row.employeeName}.`;
          this.toast.error('Save failed', msg);
          if (completed === total) this.finishSave(savedCount, failCount);
        },
      });
    }
  }

  private finishSave(savedCount: number, failCount: number): void {
    this.isSaving.set(false);
    if (savedCount > 0) {
      this.toast.success('Scores saved', `Scores saved for ${savedCount} employee(s).`);
      if (failCount === 0) this.loadEvaluationData(this.selectedProgramId(), this.selectedWeekId());
    }
    if (failCount > 0) this.toast.error('Save failed', `Failed for ${failCount} employee(s).`);
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

  private loadEvaluationData(programId: string, weekId: string, search?: string): void {
    this.isLoading.set(true);
    const params: Record<string, string | number | boolean | null> = { page: 1, limit: 100 };
    if (search) params['search'] = search;
    this.enrollmentService.listByProgram(programId, params).subscribe({
      next: (enrollments) => {
        if (enrollments.items.length === 0) { this.scoreRows.set([]); this.isLoading.set(false); return; }
        this.loadExistingEvaluations(enrollments.items);
      },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadExistingEvaluations(enrollments: Enrollment[]): void {
    const pendingRows: ScoreRow[] = [];
    let loaded = 0;
    const total = enrollments.length;
    for (const enr of enrollments) {
      this.evalService.getEnrollmentWeeklyEvaluation(Number(enr.id), 1, 100).subscribe({
        next: (res) => {
          const evals = res?.data ?? [];
          const existing = evals.find((e) => e.programWeekId === Number(this.selectedWeekId()));
          const eng = existing?.engagementScore ?? 0;
          const app = existing?.applicationScore ?? 0;
          const att = existing?.attendanceScore ?? 0;
          const row: ScoreRow = {
            enrollmentId: Number(enr.id),
            employeeId: enr.employeeId,
            employeeName: enr.employeeName,
            attendanceScore: att,
            engagementScore: eng,
            applicationScore: app,
            weeklyAverage: existing?.weeklyAverage ?? ((att + eng + app) / 3),
            hasEvaluation: !!existing,
            saving: false,
          };
          pendingRows.push(row);
          loaded++;
          if (loaded === total) {
            this.scoreRows.set([...pendingRows]);
            this.loadAttendanceScores(pendingRows);
          }
        },
        error: () => {
          pendingRows.push({
            enrollmentId: Number(enr.id),
            employeeId: enr.employeeId,
            employeeName: enr.employeeName,
            attendanceScore: 0,
            engagementScore: 0,
            applicationScore: 0,
            weeklyAverage: 0,
            hasEvaluation: false,
            saving: false,
          });
          loaded++;
          if (loaded === total) {
            this.scoreRows.set([...pendingRows]);
            this.loadAttendanceScores(pendingRows);
          }
        },
      });
    }
  }

  private loadAttendanceScores(rows: ScoreRow[]): void {
    const weekId = Number(this.selectedWeekId());
    if (!weekId) { this.isLoading.set(false); return; }
    const toCalc = rows.filter((r) => !r.hasEvaluation && r.enrollmentId);
    if (toCalc.length === 0) { this.isLoading.set(false); return; }
    let completed = 0;
    for (const row of toCalc) {
      this.evalService.getEnrollmentWeeklyEvaluationAttendanceScore(row.enrollmentId, weekId).subscribe({
        next: (score) => {
          this.scoreRows.update((r) =>
            r.map((x) =>
              x.enrollmentId === row.enrollmentId
                ? { ...x, attendanceScore: score, weeklyAverage: (score + x.engagementScore + x.applicationScore) / 3 }
                : x
            )
          );
          completed++;
          if (completed === toCalc.length) this.isLoading.set(false);
        },
        error: () => {
          completed++;
          if (completed === toCalc.length) this.isLoading.set(false);
        },
      });
    }
  }
}
