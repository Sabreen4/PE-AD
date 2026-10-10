import { Component, inject, signal, computed, OnInit, HostListener, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subject, forkJoin, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, catchError, map } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { AssignmentService } from '@features/program/services/assignment.service';
import { EmployeeAssignmentService } from '@features/program/services/employee-assignment.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { AssignmentDto } from '@features/program/models/AssignmentDto';
import { EmployeeAssignmentDto } from '@features/program/models/EmployeeAssignmentDto';
import { ToastService } from '@shared/toast/toast.service';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { DropdownModule } from 'primeng/dropdown';
import { OpenAPI } from '@core/api-gen/OpenAPI';


interface SubmissionRow {
  id: string;
  enrollmentId: number;
  programId: string;
  programName: string;
  weekId: string;
  weekNumber: number;
  weekTitle: string;
  assignmentId: string;
  assignmentTitle: string;
  maxMarks: number;
  employeeName: string;
  employeeEmail: string;
  status: string;
  fileName: string | null;
  filePath: string | null;
  fileSize: number;
  marks: number | null;
  reviewerComment: string | null;
  submittedAt: string | null;
  dueDate: string | null;
}

@Component({
  selector: 'app-assignment-submissions',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, PaginationComponent, BackNavComponent, DropdownModule],
  templateUrl: './assignment-submissions.component.html',
  styleUrl: './assignment-submissions.component.scss',
})
export class AssignmentSubmissionsComponent implements OnInit {
  private readonly programService = inject(ProgramService);
  private readonly programWeekService = inject(ProgramWeekService);
  private readonly assignmentService = inject(AssignmentService);
  private readonly empAssignmentService = inject(EmployeeAssignmentService);
  private readonly enrollmentService = inject(EnrollmentService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  constructor() {
    this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value: string) => this.onSearch(value));
  }

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly selectedWeekId = signal<string>('');

  public readonly searchQuery = signal<string>('');
  public readonly allRows = signal<SubmissionRow[]>([]);
  public readonly isLoading = signal(false);
  public readonly isEvaluating = signal(false);
  public readonly evaluateData = signal<Record<string, { marks: number; reviewerComment: string }>>({});
  public readonly currentPage = signal(1);
  public readonly pageSize = signal(25);
  public readonly selectedProgramName = computed(() => {
    const id = this.selectedProgramId();
    return this.programs().find((p) => p.id === id)?.name || '';
  });

  public readonly formattedWeeks = computed(() => {
    return [
      { id: '', label: 'All Weeks' },
      ...this.weeks().map((w) => ({
        id: w.id,
        label: `Week ${w.weekNumber}${w.topic ? ': ' + w.topic : ''}`,
      })),
    ];
  });

  public readonly filteredRows = computed(() => {
    let rows = this.allRows();
    const search = this.searchQuery().toLowerCase().trim();
    if (search) {
      rows = rows.filter(
        (r) =>
          r.employeeName.toLowerCase().includes(search) ||
          r.employeeEmail.toLowerCase().includes(search) ||
          r.assignmentTitle.toLowerCase().includes(search) ||
          r.weekTitle.toLowerCase().includes(search) ||
          r.programName.toLowerCase().includes(search) ||
          r.status.toLowerCase().includes(search)
      );
    }
    return rows;
  });

  public readonly paginatedRows = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredRows().slice(start, start + this.pageSize());
  });

  public readonly totalRecords = computed(() => this.filteredRows().length);

  public readonly unsavedCount = computed(() => {
    let count = 0;
    for (const _ in this.evaluateData()) { count++; }
    return count;
  });

  public readonly anyUnsaved = computed(() => this.unsavedCount() > 0);

  public readonly selectedRow = signal<SubmissionRow | null>(null);

  public openDetail(row: SubmissionRow): void {
    this.selectedRow.set(row);
    document.body.style.overflow = 'hidden';
  }

  public closeDetail(): void {
    this.selectedRow.set(null);
    document.body.style.overflow = '';
  }

  @HostListener('document:keydown.escape')
  public onEscape(): void {
    this.closeDetail();
  }

  public ngOnInit(): void {
    this.loadPrograms();
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

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.selectedWeekId.set('');
    this.weeks.set([]);
    this.allRows.set([]);
    this.currentPage.set(1);
    this.evaluateData.set({});
    if (programId) this.loadWeeks(programId);
  }

  public onWeekChange(weekId: string): void {
    this.selectedWeekId.set(weekId);
    this.allRows.set([]);
    this.currentPage.set(1);
    this.evaluateData.set({});
  }

  public onSearchInput(event: Event): void {
    this.search$.next((event.target as HTMLInputElement).value);
  }

  public onSearch(value: string): void {
    this.searchQuery.set(value);
    this.currentPage.set(1);
    const search = value.trim();
    if (this.selectedProgramId() && this.allRows().length === 0) {
      this.loadSubmissions();
    }
  }

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }

  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  public loadSubmissions(): void {
    const programId = this.selectedProgramId();
    if (!programId) return;
    this.isLoading.set(true);
    this.allRows.set([]);
    this.evaluateData.set({});

    const weekFilter = this.selectedWeekId();
    const programMap = new Map(this.programs().map((p) => [p.id, p.name]));

    forkJoin({
      weeks: this.programWeekService.listByProgram(programId, { page: 1, limit: 100 }).pipe(
        catchError(() => of({ items: [] as ProgramWeek[] }))
      ),
      enrollments: this.enrollmentService.listByProgram(programId, { page: 1, limit: 1000, ...(weekFilter ? {} : this.searchQuery().trim().length >= 3 ? { search: this.searchQuery().trim() } : {}) }).pipe(
        catchError(() => of({ items: [] }))
      ),
    }).subscribe({
      next: ({ weeks: weeksRes, enrollments }) => {
        console.log('[AssignmentSubmissions] API weeks response', weeksRes);
        console.log('[AssignmentSubmissions] API enrollments response', enrollments);

        const enrollmentMap = new Map<number, { employeeName: string; employeeEmail: string }>();
        for (const e of enrollments.items) {
          enrollmentMap.set(Number(e.id), { employeeName: e.employeeName, employeeEmail: '' });
        }

        const allWeeks = weekFilter
          ? weeksRes.items.filter((w) => w.id === weekFilter)
          : weeksRes.items;
        if (allWeeks.length === 0) {
          this.isLoading.set(false);
          return;
        }
        this.loadAssignmentsAndSubmissions(programId, allWeeks, programMap, enrollmentMap);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  private loadAssignmentsAndSubmissions(
    programId: string,
    weeks: ProgramWeek[],
    programMap: Map<string, string>,
    enrollmentMap: Map<number, { employeeName: string; employeeEmail: string }>,
  ): void {
    const assignmentCalls = weeks.map((week) =>
      this.assignmentService.listByWeek(programId, week.id).pipe(
        map((assignments) => {
          console.log('[AssignmentSubmissions] API assignments for week', week.id, assignments);
          return assignments.map((a) => ({ week, assignment: a }));
        }),
        catchError(() => of([] as { week: ProgramWeek; assignment: AssignmentDto }[]))
      )
    );

    forkJoin(assignmentCalls).subscribe((results) => {
      const weekAssignments = results.flat();
      if (weekAssignments.length === 0) {
        this.isLoading.set(false);
        return;
      }

      const empCalls = weekAssignments.map(({ week, assignment }) =>
        this.empAssignmentService
          .listByAssignment(programId, week.id, String(assignment.id), 1, 1000)
          .pipe(
            map((emps) => {
              console.log('[AssignmentSubmissions] API employee assignments', emps);
              emps.forEach((e) => {
                if (!e.id && !e.employeeAssignmentId) {
                  console.warn('[AssignmentSubmissions] Missing ID in employee assignment:', JSON.stringify(e));
                }
              });
              return emps.map((emp) =>
                this.toRow(programId, programMap, week, assignment, emp, enrollmentMap)
              );
            }),
            catchError(() => of([] as SubmissionRow[]))
          )
      );

      forkJoin(empCalls).subscribe((rowResults) => {
        this.allRows.set(rowResults.flat());
        this.isLoading.set(false);
      });
    });
  }

  private toRow(
    programId: string,
    programMap: Map<string, string>,
    week: ProgramWeek,
    assignment: AssignmentDto,
    emp: EmployeeAssignmentDto,
    enrollmentMap: Map<number, { employeeName: string; employeeEmail: string }>,
  ): SubmissionRow {
    const enrollmentId = emp.enrollmentId ?? 0;
    const enrollment = enrollmentMap.get(enrollmentId);

    return {
      id: String(emp.id ?? emp.employeeAssignmentId ?? ''),
      enrollmentId,
      programId,
      programName: programMap.get(programId) ?? 'Unknown Program',
      weekId: week.id,
      weekNumber: week.weekNumber,
      weekTitle: week.topic ?? '',
      assignmentId: String(assignment.id ?? ''),
      assignmentTitle: assignment.title ?? 'Unknown Assignment',
      maxMarks: assignment.maxMarks ?? 100,
      employeeName: enrollment?.employeeName ?? 'Unknown Employee',
      employeeEmail: enrollment?.employeeEmail ?? '',
      status: emp.status ?? 'pending',
      fileName: emp.fileName ?? null,
      filePath: emp.filePath ?? null,
      fileSize: emp.fileSize ?? 0,
      marks: emp.marks ?? null,
      reviewerComment: null,
      submittedAt: null,
      dueDate: assignment.endDate ?? null,
    };
  }

  public downloadFile(row: SubmissionRow): void {
    console.log(`[Download] Submission ID: ${row.id}, File: ${row.fileName}`);
    if (!row.id && !row.filePath) {
      this.toast.error('Download failed', 'No file available for this submission.');
      return;
    }

    if (row.id) {
      this.empAssignmentService
        .downloadFile(row.programId, row.weekId, row.assignmentId, row.id)
        .subscribe({
          next: (blob) => {
            const blobUrl = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = blobUrl;
            a.download = row.fileName || 'download';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(blobUrl);
            console.log(`[Download] Success: ${row.fileName}`);
          },
          error: (err) => {
            console.error(`[Download] API failed for ${row.fileName}:`, err);
            // Fallback to filePath if available
            if (row.filePath) {
              const fileUrl = row.filePath.startsWith('http')
                ? row.filePath
                : `${OpenAPI.BASE}${row.filePath.startsWith('/') ? '' : '/'}${row.filePath}`;
              const a = document.createElement('a');
              a.href = fileUrl;
              a.download = row.fileName || 'download';
              a.target = '_blank';
              document.body.appendChild(a);
              a.click();
              document.body.removeChild(a);
            } else {
              const msg =
                err.status === 404
                  ? 'File not found on server.'
                  : err.status === 403
                    ? 'Access denied — please contact your administrator.'
                    : err.status === 0
                      ? 'Network error — is the ProgramService running?'
                      : 'Download failed. Please try again.';
              this.toast.error('Download failed', msg);
            }
          },
        });
    } else if (row.filePath) {
      const a = document.createElement('a');
      a.href = row.filePath;
      a.download = row.fileName || 'download';
      a.target = '_blank';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    }
  }

  public updateMarks(rowId: string, value: string): void {
    this.evaluateData.update((d) => ({
      ...d,
      [rowId]: {
        ...(d[rowId] ?? { marks: 0, reviewerComment: '' }),
        marks: Number(value),
      },
    }));
  }

  public updateComment(rowId: string, value: string): void {
    this.evaluateData.update((d) => ({
      ...d,
      [rowId]: {
        ...(d[rowId] ?? { marks: 0, reviewerComment: '' }),
        reviewerComment: value,
      },
    }));
  }

  public getPendingData(rowId: string): { marks: number; reviewerComment: string } | null {
    return this.evaluateData()[rowId] ?? null;
  }

  public startEdit(row: SubmissionRow): void {
    this.evaluateData.update((d) => ({
      ...d,
      [row.id]: {
        marks: row.marks ?? 0,
        reviewerComment: row.reviewerComment ?? '',
      },
    }));
  }

  public evaluate(row: SubmissionRow): void {
    const data = this.evaluateData()[row.id];
    if (!data || data.marks < 0) return;
    this.isEvaluating.set(true);

    const progId = row.programId;
    const weekId = row.weekId;
    const assignId = row.assignmentId;

    this.empAssignmentService
      .evaluate(progId, weekId, assignId, row.id, {
        marks: data.marks,
        reviewerComment: data.reviewerComment || null,
      })
      .subscribe({
        next: () => {
          this.isEvaluating.set(false);
          this.evaluateData.update((d) => {
            const next = { ...d };
            delete next[row.id];
            return next;
          });
          this.allRows.update((rows) =>
            rows.map((r) =>
              r.id === row.id
                ? { ...r, marks: data.marks, reviewerComment: data.reviewerComment }
                : r
            )
          );
          this.toast.success('Score saved', `Score saved for ${row.employeeName}.`);
        },
        error: () => {
          this.isEvaluating.set(false);
          this.toast.error('Evaluation failed', 'Failed to save evaluation.');
        },
      });
  }

  public evaluateAllPending(): void {
    const pending = this.paginatedRows().filter((r) => this.getPendingData(r.id));
    if (pending.length === 0) return;
    let completed = 0;
    let successCount = 0;
    let failCount = 0;
    this.isEvaluating.set(true);

    for (const row of pending) {
      const data = this.evaluateData()[row.id];
      if (!data || data.marks < 0) {
        completed++;
        if (completed === pending.length) this.finishBatchEvaluate(successCount, failCount);
        continue;
      }

      this.empAssignmentService
        .evaluate(row.programId, row.weekId, row.assignmentId, row.id, {
          marks: data.marks,
          reviewerComment: data.reviewerComment || null,
        })
        .subscribe({
          next: () => {
            completed++;
            successCount++;
            this.evaluateData.update((d) => {
              const next = { ...d };
              delete next[row.id];
              return next;
            });
            this.allRows.update((rows) =>
              rows.map((r) =>
                r.id === row.id
                  ? { ...r, marks: data!.marks, reviewerComment: data!.reviewerComment }
                  : r
              )
            );
            if (completed === pending.length) this.finishBatchEvaluate(successCount, failCount);
          },
          error: () => {
            completed++;
            failCount++;
            if (completed === pending.length) this.finishBatchEvaluate(successCount, failCount);
          },
        });
    }
  }

  private finishBatchEvaluate(successCount: number, failCount: number): void {
    this.isEvaluating.set(false);
    if (successCount > 0 && failCount === 0) {
      this.toast.success('Evaluations saved', `Evaluated ${successCount} submission(s).`);
    } else if (failCount > 0 && successCount === 0) {
      this.toast.error('Evaluation failed', `Failed for ${failCount} submission(s).`);
    } else {
      this.toast.warning('Evaluations completed', `Evaluated ${successCount}, failed for ${failCount} submission(s).`);
    }
  }

  public isImageFile(fileName: string): boolean {
    return /\.(png|jpg|jpeg|gif|webp|svg)$/i.test(fileName);
  }

  public isVideoFile(fileName: string): boolean {
    return /\.(mp4|webm|ogg|mov|avi)$/i.test(fileName);
  }

  public isPdfFile(fileName: string): boolean {
    return /\.pdf$/i.test(fileName);
  }
}
