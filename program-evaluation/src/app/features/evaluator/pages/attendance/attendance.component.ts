import { Component, DestroyRef, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { CreateAttendanceDto } from '@features/evaluation/models/CreateAttendanceDto';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { Enrollment } from '@features/program/models/enrollment.model';
import { HttpErrorResponse } from '@angular/common/http';

import { ToastService } from '@shared/toast/toast.service';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { DropdownModule } from 'primeng/dropdown';

interface AttendanceRow {
  enrollmentId: number;
  employeeName: string;
  date: string;
  marked: boolean;
  saving: boolean;
}

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, BackNavComponent, DropdownModule],
  templateUrl: './attendance.component.html',
  styleUrl: './attendance.component.scss'
})
export class AttendanceComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly evalService: EvaluationService = inject(EvaluationService);
  private readonly toast: ToastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

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
  public readonly attendanceRows = signal<AttendanceRow[]>([]);
  public readonly attendanceDate = signal<string>('');
  public readonly searchQuery = signal<string>('');
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(25);

  public readonly filteredRows = computed(() => {
    let rows = this.attendanceRows();
    const query = this.searchQuery().trim().toLowerCase();
    if (query) {
      rows = rows.filter((r) => r.employeeName.toLowerCase().includes(query));
    }
    return rows;
  });

  public readonly totalRecords = computed(() => this.filteredRows().length);

  public readonly paginatedRows = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredRows().slice(start, start + this.pageSize());
  });

  constructor() {
    this.search$.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value: string) => {
      this.searchQuery.set(value);
      this.currentPage.set(1);
      this.reloadAttendance();
    });
  }

  public ngOnInit(): void {
    this.attendanceDate.set(new Date().toISOString().split('T')[0]);
    this.loadPrograms();
  }

  public onSearchInput(event: Event): void {
    this.search$.next((event.target as HTMLInputElement).value);
  }

  public clearSearch(input: HTMLInputElement): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    input.value = '';
    this.reloadAttendance();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.selectedWeekId.set('');
    this.searchQuery.set('');
    this.weeks.set([]);
    this.attendanceRows.set([]);
    if (programId) {
      this.loadWeeks(programId);
      this.loadEnrollments(programId);
    }
  }

  public onWeekChange(weekId: string): void {
    this.selectedWeekId.set(weekId);
    this.searchQuery.set('');
    if (weekId && this.selectedProgramId()) {
      this.loadAttendanceData(this.selectedProgramId(), weekId);
    } else {
      this.attendanceRows.set([]);
    }
  }

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }
  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  private reloadAttendance(): void {
    const pid = this.selectedProgramId();
    const wid = this.selectedWeekId();
    if (pid && wid) this.loadAttendanceData(pid, wid);
    else if (pid) this.loadEnrollments(pid);
  }

  public markPresent(enrollmentId: number): void {
    this.saveAttendance(enrollmentId, 'Present');
  }

  public markAbsent(enrollmentId: number): void {
    this.saveAttendance(enrollmentId, 'Absent');
  }

  private saveAttendance(enrollmentId: number, status: string): void {
    this.attendanceRows.update((rows) =>
      rows.map((r) => r.enrollmentId === enrollmentId ? { ...r, saving: true } : r)
    );
    const dto: CreateAttendanceDto = {
      programWeekId: Number(this.selectedWeekId()),
      attendanceDate: this.attendanceDate(),
      attendanceStatus: status,
    };
    this.evalService.postEnrollmentAttendance(enrollmentId, dto).subscribe({
      next: () => {
        this.attendanceRows.update((rows) =>
          rows.map((r) =>
            r.enrollmentId === enrollmentId ? { ...r, marked: true, saving: false } : r
          )
        );
        this.toast.success('Attendance saved', 'Attendance saved successfully.');
      },
      error: (err: HttpErrorResponse) => {
        this.attendanceRows.update((rows) =>
          rows.map((r) => r.enrollmentId === enrollmentId ? { ...r, saving: false } : r)
        );
        this.toast.error('Attendance failed', err.error?.message || 'Failed to save attendance.');
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
    const params: Record<string, string | number | boolean | null> = { page: 1, limit: 100 };
    const sq = this.searchQuery().trim();
    if (sq) params['search'] = sq;
    this.enrollmentService.listByProgram(programId, params).subscribe({
      next: (res) => {
        const rows: AttendanceRow[] = res.items.map((enr) => ({
          enrollmentId: Number(enr.id),
          employeeName: enr.employeeName,
          date: this.attendanceDate(),
          marked: false,
          saving: false,
        }));
        this.attendanceRows.set(rows);
      },
    });
  }

  private loadAttendanceData(programId: string, weekId: string): void {
    this.isLoading.set(true);
    const params: Record<string, string | number | boolean | null> = { page: 1, limit: 100 };
    const sq = this.searchQuery().trim();
    if (sq) params['search'] = sq;
    this.enrollmentService.listByProgram(programId, params).subscribe({
      next: (enrollments) => {
        if (enrollments.items.length === 0) {
          this.attendanceRows.set([]);
          this.isLoading.set(false);
          return;
        }
        const rows: AttendanceRow[] = [];
        let loaded = 0;
        const total = enrollments.items.length;
        for (const enr of enrollments.items) {
          this.evalService.getEnrollmentAttendance(Number(enr.id), 1, 100).subscribe({
            next: (res) => {
              const records = res?.data ?? [];
              const existing = records.find(
                (r) => r.programWeekId === Number(weekId) && r.attendanceDate === this.attendanceDate()
              );
              rows.push({
                enrollmentId: Number(enr.id),
                employeeName: enr.employeeName,
                date: this.attendanceDate(),
                marked: !!existing?.attendanceStatusId,
                saving: false,
              });
              loaded++;
              if (loaded === total) { this.attendanceRows.set(rows); this.isLoading.set(false); }
            },
            error: () => {
              rows.push({
                enrollmentId: Number(enr.id),
                employeeName: enr.employeeName,
                date: this.attendanceDate(),
                marked: false,
                saving: false,
              });
              loaded++;
              if (loaded === total) { this.attendanceRows.set(rows); this.isLoading.set(false); }
            },
          });
        }
      },
      error: () => { this.isLoading.set(false); },
    });
  }
}
