import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { ProgramService as MonolithProgramService } from '@features/program/services/ProgramService';
import { AssignmentService } from '@features/program/services/assignment.service';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { AssignmentDto } from '@features/program/models/AssignmentDto';
import { HttpErrorResponse } from '@angular/common/http';
import { VideoMetadataDto } from '@features/program/models/VideoMetadataDto';
import { ToastService } from '@shared/toast/toast.service';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-week-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, BackNavComponent],
  templateUrl: './week-detail.component.html',
  styleUrl: './week-detail.component.scss'
})
export class WeekDetailComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly monolithService: MonolithProgramService = inject(MonolithProgramService);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly toast: ToastService = inject(ToastService);

  public readonly week = signal<ProgramWeek | null>(null);
  public readonly assignments = signal<AssignmentDto[]>([]);
  public readonly videoMetadata = signal<VideoMetadataDto | null>(null);
  public readonly isUploading = signal(false);
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);

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
    this.programWeekService.listByProgram(this.programId).subscribe({
      next: (res) => {
        const found = res.items.find((w) => w.id === this.weekId);
        if (found) this.week.set(found);
        this.loadAssignments();
        this.loadVideo();
      },
      error: () => { this.errorMessage.set('Failed to load week.'); this.isLoading.set(false); },
    });
  }

  private loadAssignments(): void {
    this.assignmentService.listByWeek(this.programId, this.weekId).subscribe({
      next: (items) => { this.assignments.set(items ?? []); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadVideo(): void {
    this.monolithService.getProgramProgramWeekVideoDetails(Number(this.programId), Number(this.weekId)).subscribe({
      next: (meta) => { this.videoMetadata.set(meta); },
      error: () => {},
    });
  }

  public onVideoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    if (file.size > 15 * 1024 * 1024) {
      this.toast.error('File too large', 'Video file size must be less than 15 MB.');
      input.value = '';
      return;
    }
    this.isUploading.set(true);
    this.monolithService.postProgramProgramWeekVideo(Number(this.programId), Number(this.weekId), { file }).subscribe({
      next: () => { this.isUploading.set(false); this.loadVideo(); },
      error: (err: HttpErrorResponse) => { this.isUploading.set(false); this.errorMessage.set(err.error?.message || 'Upload failed.'); },
    });
  }

  public deleteVideo(): void {
    if (!confirm('Delete this recorded session?')) return;
    this.monolithService.deleteProgramProgramWeekVideo(Number(this.programId), Number(this.weekId)).subscribe({
      next: () => { this.videoMetadata.set(null); },
      error: () => {},
    });
  }

  public goBack(): void { this.router.navigate(['/evaluator/programs', this.programId, 'weeks']); }

  public isBeforeStart(assignment: AssignmentDto): boolean {
    if (!assignment.startDate) return false;
    return new Date(assignment.startDate) > new Date();
  }

  public isAfterEnd(assignment: AssignmentDto): boolean {
    if (!assignment.endDate) return false;
    return new Date(assignment.endDate) < new Date();
  }

  public isActive(assignment: AssignmentDto): boolean {
    if (!assignment.startDate || !assignment.endDate) return false;
    const now = new Date();
    return now >= new Date(assignment.startDate) && now <= new Date(assignment.endDate);
  }
}
