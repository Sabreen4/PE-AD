import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { ProgramService as MonolithProgramService } from '@features/program/services/ProgramService';
import { ProgramService } from '@features/program/services/program.service';
import { Program } from '@features/program/models/program.model';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from '@shared/toast/toast.service';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-create-week',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, BackNavComponent],
  templateUrl: './create-week.component.html',
  styleUrl: './create-week.component.scss'
})
export class CreateWeekComponent implements OnInit {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly monolithService: MonolithProgramService = inject(MonolithProgramService);
  private readonly toast: ToastService = inject(ToastService);

  public readonly program = signal<Program | null>(null);
  public readonly isSubmitting = signal(false);
  public readonly isUploading = signal(false);
  public readonly selectedVideo = signal<File | null>(null);

  public programId: string = '';

  public weekForm: FormGroup = this.fb.group({
    weekNumber: new FormControl<number>(1, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
    title: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl<string>('', { nonNullable: true }),
  });

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.programId) { this.toast.error('Week', 'Program ID not found.'); return; }
    this.programService.getById(this.programId).subscribe({
      next: (prog) => { this.program.set(prog); },
      error: () => { this.toast.error('Week', 'Failed to load program.'); },
    });
  }

  public get f(): Record<string, FormControl> { return this.weekForm.controls as Record<string, FormControl>; }

  public onVideoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      const file = input.files[0];
      if (file.size > 15 * 1024 * 1024) {
        this.toast.error('File too large', 'Video file size must be less than 15 MB.');
        input.value = '';
        this.selectedVideo.set(null);
        return;
      }
      this.selectedVideo.set(file);
    }
  }

  public onSubmit(): void {
    if (this.weekForm.invalid || this.isSubmitting() || !this.programId) return;
    this.isSubmitting.set(true);
    const value = this.weekForm.value;

    this.programWeekService.create(this.programId, {
      weekNumber: value.weekNumber,
      title: value.title,
      description: value.description || null,
    } as {} as Partial<ProgramWeek>).subscribe({
      next: (created) => {
        const vid = this.selectedVideo();
        const weekId = String(created.id);
        if (vid) {
          this.isUploading.set(true);
          this.monolithService.postProgramProgramWeekVideo(
            this.toSafeId(this.programId), this.toSafeId(weekId), { file: vid }
          ).subscribe({
            next: () => {
              this.toast.success('Week', 'Week created with video.');
              this.router.navigate(['/evaluator/programs', this.programId, 'weeks', weekId]);
            },
            error: (err: HttpErrorResponse) => {
              this.isUploading.set(false);
              this.isSubmitting.set(false);
              this.toast.warning('Week', err.error?.message || 'Week created, but video upload failed.');
            },
          });
        } else {
          this.toast.success('Week', 'Week created successfully.');
          this.router.navigate(['/evaluator/programs', this.programId, 'weeks', weekId]);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.toast.error('Week', err.error?.message || 'Failed to create week.');
      },
    });
  }

  private toSafeId(value: string | number): number {
    const n = Number(value);
    return isNaN(n) ? (value as {} as number) : n;
  }
}
