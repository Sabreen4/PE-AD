import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssignmentService } from '@features/program/services/assignment.service';
import { EmployeeAssignmentService } from '@features/program/services/employee-assignment.service';
import { AssignmentDetailDto } from '@features/program/models/AssignmentDetailDto';
import { EmployeeAssignmentDto } from '@features/program/models/EmployeeAssignmentDto';
import { EmployeeAssignmentDetailDto } from '@features/program/models/EmployeeAssignmentDetailDto';
import { HttpErrorResponse } from '@angular/common/http';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-assignment-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, BackNavComponent],
  templateUrl: './assignment-detail.component.html',
  styleUrl: './assignment-detail.component.scss'
})
export class AssignmentDetailComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly empAssignmentService: EmployeeAssignmentService = inject(EmployeeAssignmentService);

  public readonly assignment = signal<AssignmentDetailDto | null>(null);
  public readonly submissions = signal<EmployeeAssignmentDto[]>([]);
  public readonly isLoading = signal(false);
  public readonly isEvaluating = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly evaluateData = signal<Record<string, { marks: number; reviewerComment: string }>>({});
  public readonly pendingReviewCount = computed(() => this.submissions().filter(s => s.marks == null).length);

  public programId: string = '';
  public weekId: string = '';
  public assignmentId: string = '';

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    this.weekId = this.route.snapshot.paramMap.get('weekId') ?? '';
    this.assignmentId = this.route.snapshot.paramMap.get('assignmentId') ?? '';
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.assignmentService.getById(this.programId, this.weekId, this.assignmentId).subscribe({
      next: (a) => {
        this.assignment.set(a);
        this.loadSubmissions();
      },
      error: () => { this.errorMessage.set('Failed to load assignment.'); this.isLoading.set(false); },
    });
  }

  private loadSubmissions(): void {
    this.empAssignmentService.listByAssignment(this.programId, this.weekId, this.assignmentId).subscribe({
      next: (items) => { this.submissions.set(items ?? []); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  public updateMarks(submissionId: string, value: string): void {
    this.evaluateData.update((d) => ({ ...d, [submissionId]: { ...(d[submissionId] ?? { marks: 0, reviewerComment: '' }), marks: Number(value) } }));
  }

  public updateComment(submissionId: string, value: string): void {
    this.evaluateData.update((d) => ({ ...d, [submissionId]: { ...(d[submissionId] ?? { marks: 0, reviewerComment: '' }), reviewerComment: value } }));
  }

  public getComment(s: EmployeeAssignmentDto): string | null {
    return (s as EmployeeAssignmentDetailDto).reviewerComment ?? null;
  }

  public evaluate(submission: EmployeeAssignmentDto): void {
    const data = this.evaluateData()[submission.id ?? ''];
    if (!data || data.marks < 0) return;
    this.isEvaluating.set(true);
    this.empAssignmentService.evaluate(this.programId, this.weekId, this.assignmentId, String(submission.id), {
      marks: data.marks,
      reviewerComment: data.reviewerComment || null,
    }).subscribe({
      next: () => { this.isEvaluating.set(false); this.loadSubmissions(); },
      error: (err: HttpErrorResponse) => { this.isEvaluating.set(false); this.errorMessage.set(err.error?.message || 'Evaluation failed.'); },
    });
  }

  public goBack(): void { this.router.navigate(['/evaluator/programs', this.programId, 'weeks', this.weekId]); }

  public isBeforeStart(): boolean {
    const a = this.assignment();
    if (!a?.startDate) return false;
    return new Date(a.startDate) > new Date();
  }

  public isAfterEnd(): boolean {
    const a = this.assignment();
    if (!a?.endDate) return false;
    return new Date(a.endDate) < new Date();
  }

  public isActive(): boolean {
    const a = this.assignment();
    if (!a?.startDate || !a?.endDate) return false;
    const now = new Date();
    return now >= new Date(a.startDate) && now <= new Date(a.endDate);
  }
}
