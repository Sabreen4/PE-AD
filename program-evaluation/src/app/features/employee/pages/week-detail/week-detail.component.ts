import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramService as MonolithProgramService } from '@features/program/services/ProgramService';
import { AssignmentService } from '@features/program/services/assignment.service';
import { EmployeeAssignmentService } from '@features/program/services/employee-assignment.service';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { AssignmentDto } from '@features/program/models/AssignmentDto';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-employee-week-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, BackNavComponent],
  templateUrl: './week-detail.component.html',
  styleUrl: './week-detail.component.scss'
})
export class EmployeeWeekDetailComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly monolithService: MonolithProgramService = inject(MonolithProgramService);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly empAssignmentService: EmployeeAssignmentService = inject(EmployeeAssignmentService);
  private readonly sanitizer: DomSanitizer = inject(DomSanitizer);

  public readonly program = signal<Program | null>(null);
  public readonly week = signal<ProgramWeek | null>(null);
  public readonly assignments = signal<AssignmentDto[]>([]);
  public readonly videoUrl = signal<SafeResourceUrl | null>(null);
  public readonly videoLoading = signal(false);
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly isForbidden = signal(false);

  public programId: string = '';
  public weekId: string = '';

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    this.weekId = this.route.snapshot.paramMap.get('weekId') ?? '';
    if (!this.programId || !this.weekId) { this.errorMessage.set('Invalid route.'); return; }
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.programService.getById(this.programId).subscribe({
      next: (prog) => {
        this.program.set(prog);
        this.loadWeek();
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view this week.'); }
        else { this.errorMessage.set('Failed to load.'); }
        this.isLoading.set(false);
      },
    });
  }

  private loadWeek(): void {
    this.programWeekService.listByProgram(this.programId).subscribe({
      next: (res) => {
        const found = res.items.find((w) => w.id === this.weekId);
        if (found) this.week.set(found);
        this.loadAssignments();
        this.loadVideo();
      },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadAssignments(): void {
    this.assignmentService.listByWeek(this.programId, this.weekId).subscribe({
      next: (items) => { this.assignments.set(items ?? []); this.isLoading.set(false); },
      error: (err: HttpErrorResponse) => {
        if (err.status === 403) { this.errorMessage.set('You do not have permission to view assignments.'); }
        this.isLoading.set(false);
      },
    });
  }

  private loadVideo(): void {
    this.videoLoading.set(true);
    this.monolithService.getProgramProgramWeekVideo(Number(this.programId), Number(this.weekId)).subscribe({
      next: (blob: Blob) => {
        const url = URL.createObjectURL(blob);
        this.videoUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(url));
        this.videoLoading.set(false);
      },
      error: () => { this.videoLoading.set(false); },
    });
  }

  public goBack(): void { this.router.navigate(['/employee/programs', this.programId]); }

  public assignmentStatus(a: AssignmentDto): 'upcoming' | 'active' | 'closed' {
    const now = new Date();
    if (a.startDate && new Date(a.startDate) > now) return 'upcoming';
    if (a.endDate && new Date(a.endDate) < now) return 'closed';
    return 'active';
  }
}
