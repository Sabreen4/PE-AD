import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { AssignmentService } from '@features/program/services/assignment.service';
import { EmployeeAssignmentService } from '@features/program/services/employee-assignment.service';
import { ProgramService as MonolithProgramService } from '@features/program/services/ProgramService';
import { AssignmentDetailDto } from '@features/program/models/AssignmentDetailDto';
import { EmployeeAssignmentDetailDto } from '@features/program/models/EmployeeAssignmentDetailDto';
import { VideoMetadataDto } from '@features/program/models/VideoMetadataDto';
import { HttpErrorResponse } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ToastService } from '@shared/toast/toast.service';

type AssignmentStatus = 'locked' | 'active' | 'submitted' | 'expired';

@Component({
  selector: 'app-assignment-view',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, BackNavComponent],
  templateUrl: './assignment-view.component.html',
  styleUrl: './assignment-view.component.scss'
})
export class AssignmentViewComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly empAssignmentService: EmployeeAssignmentService = inject(EmployeeAssignmentService);
  private readonly monolithService: MonolithProgramService = inject(MonolithProgramService);
  private readonly sanitizer: DomSanitizer = inject(DomSanitizer);
  private readonly toast: ToastService = inject(ToastService);

  public readonly assignment = signal<AssignmentDetailDto | null>(null);
  public readonly mySubmission = signal<EmployeeAssignmentDetailDto | null>(null);
  public readonly isLoading = signal(false);
  public readonly isSubmitting = signal(false);
  public readonly selectedFile = signal<File | null>(null);
  public readonly videoUrl = signal<SafeResourceUrl | null>(null);
  public readonly videoMeta = signal<VideoMetadataDto | null>(null);
  public readonly videoLoading = signal(false);

  public programId: string = '';
  public weekId: string = '';
  public assignmentId: string = '';

  public readonly status = computed<AssignmentStatus>(() => {
    const a = this.assignment();
    const sub = this.mySubmission();
    if (!a) return 'locked';
    if (sub?.status === 'submitted' || sub?.status === 'evaluated' || sub?.status === 'reviewed') return 'submitted';
    const now = new Date();
    if (a.startDate && new Date(a.startDate) > now) return 'locked';
    if (a.endDate && new Date(a.endDate) < now) return 'expired';
    return 'active';
  });

  public readonly canSubmit = computed(() => {
    const s = this.status();
    const fileErr = this.fileError();
    return s === 'active' && !fileErr && this.selectedFile() != null;
  });

  public readonly daysRemaining = computed(() => {
    const a = this.assignment();
    if (!a?.endDate) return null;
    const diff = new Date(a.endDate).getTime() - Date.now();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  });

  public readonly acceptTypes = computed(() => {
    const a = this.assignment();
    if (!a?.fileType) return '';
    const raw = a.fileType.toLowerCase().replace(/^\.+/, '');
    return `.${raw}`;
  });

  public readonly fileError = computed(() => {
    const file = this.selectedFile();
    const a = this.assignment();
    if (!file || !a) return null;

    const ext = file.name.split('.').pop()?.toLowerCase();
    if (a.fileType) {
      const allowed = a.fileType.toLowerCase().replace(/^\.+/, '');
      if (ext !== allowed) {
        return `File type ".${ext}" not allowed. Only ".${allowed}" files are accepted.`;
      }
    }

    const maxBytes = 10 * 1024 * 1024;
    if (file.size > maxBytes) {
      const mb = (file.size / (1024 * 1024)).toFixed(1);
      return `File too large (${mb} MB). Maximum size is 10 MB.`;
    }

    return null;
  });

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
        this.loadMySubmission();
        this.loadVideo();
      },
      error: () => { this.toast.error('Assignment', 'Failed to load assignment.'); this.isLoading.set(false); },
    });
  }

  private loadVideo(): void {
    this.videoLoading.set(true);
    this.monolithService.getProgramProgramWeekVideoDetails(Number(this.programId), Number(this.weekId)).subscribe({
      next: (meta) => {
        this.videoMeta.set(meta);
        if (meta?.videoName) {
          this.monolithService.getProgramProgramWeekVideo(Number(this.programId), Number(this.weekId)).subscribe({
            next: (blob: Blob) => {
              const url = URL.createObjectURL(blob);
              this.videoUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(url));
              this.videoLoading.set(false);
            },
            error: () => { this.videoLoading.set(false); },
          });
        } else {
          this.videoLoading.set(false);
        }
      },
      error: () => { this.videoLoading.set(false); },
    });
  }

  private loadMySubmission(): void {
    this.empAssignmentService.getMyAssignment(this.programId, this.weekId, this.assignmentId).subscribe({
      next: (sub) => {
        if (sub) this.mySubmission.set(sub);
        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); },
    });
  }

  public onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile.set(input.files[0]);
    }
  }

  public triggerFileUpload(): void {
    const input = document.createElement('input');
    input.type = 'file';
    const accept = this.acceptTypes();
    if (accept) input.accept = accept;
    input.style.display = 'none';
    input.addEventListener('change', (event: Event) => {
      this.onFileSelected(event);
      input.remove();
    });
    document.body.appendChild(input);
    input.click();
  }

  public submitAssignment(): void {
    const fileErr = this.fileError();
    if (!this.canSubmit() || fileErr) { if (fileErr) this.toast.warning('Submission', fileErr); return; }
    const file = this.selectedFile();
    if (!file) { this.toast.warning('Submission', 'Please select a file.'); return; }
    this.isSubmitting.set(true);

    if (this.mySubmission()?.id) {
      const sub = this.mySubmission()!;
      this.empAssignmentService.submit(this.programId, this.weekId, this.assignmentId, String(sub.id), file).subscribe({
        next: (res) => {
          this.isSubmitting.set(false);
          this.toast.success('Assignment', 'Assignment submitted successfully.');
          this.mySubmission.set(res as {} as EmployeeAssignmentDetailDto);
          this.selectedFile.set(null);
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.toast.error('Submission', err.error?.message || 'Submission failed.');
        },
      });
    } else {
      this.empAssignmentService.createMyAssignment(this.programId, this.weekId, this.assignmentId).subscribe({
        next: (created) => {
          this.empAssignmentService.submit(this.programId, this.weekId, this.assignmentId, String(created.id), file).subscribe({
            next: (res) => {
              this.isSubmitting.set(false);
              this.toast.success('Assignment', 'Assignment submitted successfully.');
              this.mySubmission.set(res as {} as EmployeeAssignmentDetailDto);
              this.selectedFile.set(null);
            },
            error: (err: HttpErrorResponse) => {
              this.isSubmitting.set(false);
              this.toast.error('Submission', err.error?.message || 'Submission failed.');
            },
          });
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.toast.error('Submission', err.error?.message || 'Failed to create submission.');
        },
      });
    }
  }

  public daysLabel(days: number): string {
    if (days === 0) return 'Last day';
    if (days === 1) return '1 day left';
    return `${days} days left`;
  }

  public goBack(): void { this.router.navigate(['/employee/programs', this.programId, 'weeks', this.weekId]); }

  public statusLabel(): string {
    switch (this.status()) {
      case 'locked': return 'Locked';
      case 'active': return 'Active';
      case 'submitted': return 'Submitted';
      case 'expired': return 'Expired';
    }
  }
}
