import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { AssignmentService } from '@features/program/services/assignment.service';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { EnrollmentDetailDto } from '@features/program/models/EnrollmentDetailDto';
import { AssignmentDto } from '@features/program/models/AssignmentDto';

@Component({
  selector: 'app-employee-program-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, BackNavComponent],
  templateUrl: './program-detail.component.html',
  styleUrl: './program-detail.component.scss'
})
export class EmployeeProgramDetailComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);

  public readonly program = signal<Program | null>(null);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly assignments = signal<AssignmentDto[]>([]);
  public readonly enrollment = signal<EnrollmentDetailDto | null>(null);
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly isForbidden = signal(false);

  public programId: string = '';

  public readonly sortedWeeks = computed(() => this.weeks().sort((a, b) => a.weekNumber - b.weekNumber));
  public readonly completedWeeks = computed(() => this.sortedWeeks().filter((w) => this.weekStatus(w) === 'completed').length);
  public readonly totalWeeks = computed(() => this.sortedWeeks().length);
  public readonly progressPercent = computed(() => {
    if (this.totalWeeks() === 0) return 0;
    return Math.round((this.completedWeeks() / this.totalWeeks()) * 100);
  });
  public readonly currentWeekNum = computed(() => {
    const current = this.sortedWeeks().find((w) => this.weekStatus(w) === 'current');
    return current?.weekNumber ?? this.completedWeeks() + 1;
  });

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.programId) { this.errorMessage.set('Program not found.'); return; }
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.programService.getById(this.programId).subscribe({
      next: (prog) => {
        this.program.set(prog);
        this.loadWeeks();
        this.loadEnrollment();
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view this program.'); }
        else { this.errorMessage.set('Failed to load program.'); }
        this.isLoading.set(false);
      },
    });
  }

  private loadWeeks(): void {
    this.programWeekService.listByProgram(this.programId).subscribe({
      next: (res) => {
        this.weeks.set(res.items.sort((a, b) => a.weekNumber - b.weekNumber));
        this.loadAssignments();
      },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadAssignments(): void {
    const weekIds = this.weeks().map((w) => w.id);
    if (weekIds.length === 0) { this.isLoading.set(false); return; }
    let all: AssignmentDto[] = [];
    let completed = 0;
    for (const weekId of weekIds) {
      this.assignmentService.listByWeek(this.programId, weekId).subscribe({
        next: (items) => { all = [...all, ...(items ?? [])]; completed++; if (completed === weekIds.length) { this.assignments.set(all); this.isLoading.set(false); } },
        error: () => { completed++; if (completed === weekIds.length) { this.assignments.set(all); this.isLoading.set(false); } },
      });
    }
  }

  private loadEnrollment(): void {
    this.enrollmentService.getMyEnrollment(this.programId).subscribe({
      next: (res) => {
        if (res) this.enrollment.set(res);
      },
    });
  }

  public goBack(): void { this.router.navigate(['/employee/programs']); }

  public weekStatus(week: ProgramWeek): 'completed' | 'current' | 'upcoming' {
    const enr = this.enrollment();
    if (!enr) return 'upcoming';
    const currentWeek = enr.currentWeek ?? 1;
    if (week.weekNumber < currentWeek) return 'completed';
    if (week.weekNumber === currentWeek) return 'current';
    return 'upcoming';
  }

  public weekAssignments(weekId: string): AssignmentDto[] {
    return this.assignments().filter((a) => String(a.programWeekId) === weekId);
  }

  public isBeforeStart(a: AssignmentDto): boolean {
    if (!a.startDate) return false;
    return new Date(a.startDate) > new Date();
  }

  public isAfterEnd(a: AssignmentDto): boolean {
    if (!a.endDate) return false;
    return new Date(a.endDate) < new Date();
  }

  public isActive(a: AssignmentDto): boolean {
    return !this.isBeforeStart(a) && !this.isAfterEnd(a);
  }

  public hasActiveAssignment(weekId: string): boolean {
    return this.weekAssignments(weekId).some((a) => this.isActive(a));
  }

  public countWeekAssignments(weekId: string): number {
    return this.weekAssignments(weekId).length;
  }
}
