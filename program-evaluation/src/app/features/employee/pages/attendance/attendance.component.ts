import { Component, DestroyRef, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ProgramService } from '@features/program/services/program.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { Program } from '@features/program/models/program.model';
import { AttendanceDto } from '@features/evaluation/models/AttendanceDto';
import { EnrollmentDetailDto } from '@features/program/models/EnrollmentDetailDto';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { DropdownModule } from 'primeng/dropdown';

interface AttendanceRecord {
  weekLabel: string;
  date: string;
  status: string;
}

@Component({
  selector: 'app-employee-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, BackNavComponent, DropdownModule],
  templateUrl: './attendance.component.html',
  styleUrl: './attendance.component.scss',
})
export class EmployeeAttendanceComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly evalService: EvaluationService = inject(EvaluationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  public readonly programs = signal<Program[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly isForbidden = signal(false);
  public readonly enrollment = signal<EnrollmentDetailDto | null>(null);
  public readonly allRecords = signal<AttendanceRecord[]>([]);
  public readonly searchQuery = signal<string>('');
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(25);

  public readonly filteredRecords = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.allRecords();
    return this.allRecords().filter((r) => r.weekLabel.toLowerCase().includes(q) || r.date.toLowerCase().includes(q));
  });

  public readonly paginatedRecords = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredRecords().slice(start, start + this.pageSize());
  });

  public readonly totalRecords = computed(() => this.filteredRecords().length);
  public readonly presentCount = computed(() => this.allRecords().filter((r) => r.status === 'Present').length);
  public readonly absentCount = computed(() => this.allRecords().filter((r) => r.status === 'Absent').length);
  public readonly attendancePercentage = computed(() => {
    const marked = this.allRecords().filter((r) => r.status === 'Present' || r.status === 'Absent').length;
    const present = this.presentCount();
    return marked > 0 ? Math.round((present / marked) * 100) : null;
  });

  constructor() {
    this.search$.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value: string) => {
      this.searchQuery.set(value);
      this.currentPage.set(1);
    });
  }

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onSearchInput(event: Event): void {
    this.search$.next((event.target as HTMLInputElement).value);
  }

  public clearSearch(input: HTMLInputElement): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    input.value = '';
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.allRecords.set([]);
    this.searchQuery.set('');
    this.errorMessage.set(null);
    this.isForbidden.set(false);
    this.enrollment.set(null);
    if (programId) this.loadAttendance(programId);
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => { this.programs.set(res.items); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadAttendance(programId: string): void {
    this.isLoading.set(true);
    this.enrollmentService.getMyEnrollment(programId).subscribe({
      next: (enrollment) => {
        if (!enrollment) {
          this.errorMessage.set('You are not enrolled in this program.');
          this.isLoading.set(false);
          return;
        }
        this.enrollment.set(enrollment);
        this.evalService.getEnrollmentAttendance(Number(enrollment.id), 1, 100).subscribe({
          next: (res) => {
            const data: AttendanceDto[] = res?.data ?? [];
            const records: AttendanceRecord[] = data.map((a) => ({
              weekLabel: a.programWeekId ? `Week ${a.programWeekId}` : '—',
              date: a.attendanceDate ?? '—',
              status: a.attendanceStatusId === 'Present' ? 'Present' : (a.attendanceStatusId === 'Absent' ? 'Absent' : '—'),
            }));
            records.sort((a, b) => a.date.localeCompare(b.date));
            this.allRecords.set(records);
            this.isLoading.set(false);
          },
          error: (err: HttpErrorResponse) => {
            this.isLoading.set(false);
            if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view attendance records. This endpoint may require backend authorization for employee self-access.'); }
            else { this.errorMessage.set('Failed to load attendance.'); }
          },
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to access this enrollment.'); }
        else { this.errorMessage.set('You are not enrolled in this program.'); }
      },
    });
  }

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }
  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }
}
