import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { AssignmentService } from '@features/program/services/assignment.service';
import { MetadataService } from '@features/program/services/metadata.service';
import { HttpErrorResponse } from '@angular/common/http';
import { FileTypeResponseDto } from '@features/program/models/FileTypeResponseDto';
import { ToastService } from '@shared/toast/toast.service';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { DropdownModule } from 'primeng/dropdown';

@Component({
  selector: 'app-create-assignment',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, BackNavComponent, DropdownModule],
  templateUrl: './create-assignment.component.html',
  styleUrl: './create-assignment.component.scss'
})
export class CreateAssignmentComponent implements OnInit {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly metadataService: MetadataService = inject(MetadataService);
  private readonly toast: ToastService = inject(ToastService);

  public readonly isSubmitting = signal(false);
  public readonly isLoadingFileTypes = signal(false);
  public readonly selectedFileTypeKey = signal<string | null>(null);
  public readonly fileTypeOptions = signal<FileTypeResponseDto[]>([]);

  public readonly formattedFileTypes = computed(() => {
    return [
      { key: null, label: 'Any file type' },
      ...this.fileTypeOptions().map((opt) => ({
        key: opt.key,
        label: `${opt.displayName} (.${opt.key.toLowerCase()})`,
      })),
    ];
  });

  public assignmentForm: FormGroup = this.fb.group({
    title: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl<string>('', { nonNullable: true }),
    startDate: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    endDate: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    maxMarks: new FormControl<number>(100, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
  });

  public programId: string = '';
  public weekId: string = '';

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    this.weekId = this.route.snapshot.paramMap.get('weekId') ?? '';
    this.loadFileTypes();
  }

  private loadFileTypes(): void {
    this.isLoadingFileTypes.set(true);
    this.metadataService.getFileTypes().subscribe({
      next: (types) => { this.fileTypeOptions.set(types ?? []); this.isLoadingFileTypes.set(false); },
      error: () => { this.isLoadingFileTypes.set(false); },
    });
  }

  public get f(): Record<string, FormControl> { return this.assignmentForm.controls as Record<string, FormControl>; }

  public selectFileType(key: string | null): void {
    this.selectedFileTypeKey.set(key);
  }

  public onSubmit(): void {
    if (this.assignmentForm.invalid || this.isSubmitting()) return;
    const value = this.assignmentForm.value;
    if (new Date(value.startDate) >= new Date(value.endDate)) {
      this.toast.warning('Assignment', 'End date must be after start date.');
      return;
    }
    this.isSubmitting.set(true);
    this.assignmentService.create(this.programId, this.weekId, {
      title: value.title,
      description: value.description || null,
      startDate: new Date(value.startDate).toISOString(),
      endDate: new Date(value.endDate).toISOString(),
      maxMarks: value.maxMarks,
      fileType: this.selectedFileTypeKey(),
    }).subscribe({
      next: () => {
        this.toast.success('Assignment', 'Assignment created successfully.');
        this.router.navigate(['/evaluator/programs', this.programId, 'weeks', this.weekId]);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.toast.error('Assignment', err.error?.message || 'Failed to create assignment.');
      },
    });
  }
}
