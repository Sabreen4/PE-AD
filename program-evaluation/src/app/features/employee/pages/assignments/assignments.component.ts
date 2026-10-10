import { Component, inject, signal, OnInit, computed, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SkeletonModule } from 'primeng/skeleton';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';
import { FileUploadModule } from 'primeng/fileupload';
import { DropdownModule } from 'primeng/dropdown';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { AssignmentService } from '@features/program/services/assignment.service';
import { EmployeeAssignmentService } from '@features/program/services/employee-assignment.service';
import { MetadataService } from '@features/program/services/metadata.service';
import { ProgramService as MonolithProgramService } from '@features/program/services/ProgramService';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { AssignmentDto } from '@features/program/models/AssignmentDto';
import { EmployeeAssignmentDetailDto } from '@features/program/models/EmployeeAssignmentDetailDto';
import { FileTypeResponseDto } from '@features/program/models/FileTypeResponseDto';
import { ToastService } from '@shared/toast/toast.service';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { VideoPlayerModalComponent, VideoPlayerData } from '../../components/video-player-modal/video-player-modal.component';

interface AssignmentCardData {
  assignment: AssignmentDto;
  week: ProgramWeek;
  program: Program;
  submission: EmployeeAssignmentDetailDto | null;
  fileTypeLabel: string;
  status: 'upcoming' | 'active' | 'expired' | 'submitted' | 'reviewed';
  isBeforeStart: boolean;
  isAfterEnd: boolean;
  canSubmit: boolean;
  videoAvailable: boolean;
  videoName: string | null;
  videoDisplayTitle: string | null;
  videoDuration: string | null;
}

type FilterTab = 'all' | 'pending' | 'submitted' | 'reviewed' | 'expired';

interface Tab { key: FilterTab; label: string; }

@Component({
  selector: 'app-employee-assignments',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink, MatIconModule,
    CardModule, ButtonModule, TagModule, SkeletonModule,
    AvatarModule, BadgeModule, RippleModule, TooltipModule, FileUploadModule,
    VideoPlayerModalComponent, BackNavComponent, DropdownModule
  ],
  templateUrl: './assignments.component.html',
  styleUrl: './assignments.component.scss'
})
export class EmployeeAssignmentsComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly empAssignmentService: EmployeeAssignmentService = inject(EmployeeAssignmentService);
  private readonly metadataService: MetadataService = inject(MetadataService);
  private readonly monolithService: MonolithProgramService = inject(MonolithProgramService);
  private readonly sanitizer: DomSanitizer = inject(DomSanitizer);
  private readonly toast: ToastService = inject(ToastService);

  @ViewChild('videoPlayer') videoPlayerModal!: VideoPlayerModalComponent;

  public readonly programs = signal<Program[]>([]);
  public readonly allCards = signal<AssignmentCardData[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly activeFilter = signal<FilterTab>('all');
  public readonly isLoading = signal(false);
  public readonly isLoadingCards = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly isForbidden = signal(false);
  public readonly fileTypes = signal<FileTypeResponseDto[]>([]);
  public readonly selectedFileMap = signal<Map<string, File>>(new Map());
  public readonly submitStates = signal<Map<string, { submitting: boolean; error: string | null; success: string | null }>>(new Map());

  public readonly tabs: Tab[] = [
    { key: 'all', label: 'All' },
    { key: 'pending', label: 'Pending' },
    { key: 'submitted', label: 'Submitted' },
    { key: 'reviewed', label: 'Reviewed' },
    { key: 'expired', label: 'Expired' },
  ];

  public tabCountForKey(key: FilterTab): number {
    return this.tabCounts()[key];
  }

  public triggerFileUpload(cardKey: string): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.style.display = 'none';
    input.addEventListener('change', (event: Event) => {
      this.onFileSelected(event, cardKey);
      input.remove();
    });
    document.body.appendChild(input);
    input.click();
  }

  public filteredCards = computed(() => {
    const filter = this.activeFilter();
    const cards = this.allCards();
    switch (filter) {
      case 'pending': return cards.filter((c) => c.status === 'upcoming' || c.status === 'active');
      case 'submitted': return cards.filter((c) => c.status === 'submitted');
      case 'reviewed': return cards.filter((c) => c.status === 'reviewed');
      case 'expired': return cards.filter((c) => c.status === 'expired');
      default: return cards;
    }
  });

  public readonly tabCounts = computed(() => {
    const cards = this.allCards();
    return {
      all: cards.length,
      pending: cards.filter((c) => c.status === 'upcoming' || c.status === 'active').length,
      submitted: cards.filter((c) => c.status === 'submitted').length,
      reviewed: cards.filter((c) => c.status === 'reviewed').length,
      expired: cards.filter((c) => c.status === 'expired').length,
    };
  });

  public readonly metrics = computed(() => {
    const cards = this.allCards();
    const pending = cards.filter((c) => c.status === 'upcoming' || c.status === 'active').length;
    const expired = cards.filter((c) => c.status === 'expired').length;
    const submitted = cards.filter((c) => c.status === 'submitted').length;
    const reviewed = cards.filter((c) => c.status === 'reviewed').length;
    const total = cards.length;
    const completionRate = total > 0 ? Math.round(((submitted + reviewed) / total) * 100) : 0;
    const scored = cards.filter((c) => c.submission?.marks != null);
    const avgScore = scored.length > 0
      ? Math.round(scored.reduce((s, c) => s + (c.submission!.marks ?? 0), 0) / scored.length)
      : 0;
    const earliestDue = cards
      .filter((c) => c.assignment.endDate && (c.status === 'active' || c.status === 'upcoming'))
      .sort((a, b) => new Date(a.assignment.endDate!).getTime() - new Date(b.assignment.endDate!).getTime())[0];
    return { pending, expired, submitted, reviewed, total, completionRate, avgScore, earliestDue };
  });

  public ngOnInit(): void {
    this.loadFileTypes();
    this.loadPrograms();
  }

  private loadFileTypes(): void {
    this.metadataService.getFileTypes().subscribe({
      next: (types) => this.fileTypes.set(types ?? []),
      error: () => {},
    });
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.activeFilter.set('all');
    if (programId) this.loadAssignments(programId);
    else this.allCards.set([]);
  }

  public setFilter(tab: FilterTab): void {
    this.activeFilter.set(tab);
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => {
        this.programs.set(res.items);
        this.isLoading.set(false);
        if (res.items.length > 0) {
          this.selectedProgramId.set(res.items[0].id);
          this.loadAssignments(res.items[0].id);
        }
      },
      error: () => { this.errorMessage.set('Failed to load programs.'); this.isLoading.set(false); },
    });
  }

  private loadAssignments(programId: string): void {
    this.isLoadingCards.set(true);
    this.errorMessage.set(null);

    forkJoin({
      weeks: this.programWeekService.listByProgram(programId),
      program: this.programService.getById(programId),
    }).subscribe({
      next: (res) => {
        const weeks = res.weeks.items;
        const prog = res.program;
        const program: Program = { ...(prog ?? ({} as Program)), id: programId };

        if (weeks.length === 0) { this.allCards.set([]); this.isLoadingCards.set(false); return; }

        const assignmentCalls = weeks.map((w) =>
          this.assignmentService.listByWeek(programId, w.id).pipe(
            map((items) => ({ week: w, items: items ?? [] })),
            catchError(() => of({ week: w, items: [] }))
          )
        );

        forkJoin(assignmentCalls).subscribe({
          next: (aResults) => {
            const flat: { week: ProgramWeek; assignment: AssignmentDto }[] = [];
            for (const r of aResults) {
              for (const a of r.items) {
                flat.push({ week: r.week, assignment: a });
              }
            }

            if (flat.length === 0) { this.allCards.set([]); this.isLoadingCards.set(false); return; }

            const subCalls = flat.map((item) =>
              this.empAssignmentService.getMyAssignment(programId, item.week.id, String(item.assignment.id)).pipe(
                map((sub) => ({ key: `${item.week.id}-${item.assignment.id}`, submission: sub })),
                catchError(() => of({ key: `${item.week.id}-${item.assignment.id}`, submission: null }))
              )
            );

            forkJoin(subCalls).subscribe({
              next: (subResults) => {
                const subMap = new Map<string, EmployeeAssignmentDetailDto | null>();
                for (const sr of subResults) {
                  subMap.set(sr.key, sr.submission);
                }

                const cards: AssignmentCardData[] = flat.map((item) => {
                  const key = `${item.week.id}-${item.assignment.id}`;
                  const submission = subMap.get(key) ?? null;
                  const now = new Date();
                  const startDate = item.assignment.startDate ? new Date(item.assignment.startDate) : null;
                  const endDate = item.assignment.endDate ? new Date(item.assignment.endDate) : null;
                  const isBeforeStart = startDate ? startDate > now : false;
                  const isAfterEnd = endDate ? endDate < now : false;
                  const status = this.computeCardStatus(item.assignment, submission);

                  const ftKey = item.assignment.fileType ?? '';
                  const ft = this.fileTypes().find((f) => f.key === ftKey);
                  const fileTypeLabel = ft?.displayName ?? (ftKey ? `.${ftKey.toLowerCase()}` : 'Any');

                  const videoAvailable = !!(item.week.videoName);
                  return {
                    assignment: item.assignment,
                    week: item.week,
                    program,
                    submission,
                    fileTypeLabel,
                    status,
                    isBeforeStart,
                    isAfterEnd,
                    canSubmit: !isBeforeStart && !isAfterEnd && status !== 'submitted' && status !== 'reviewed',
                    videoAvailable,
                    videoName: item.week.videoName ?? null,
                    videoDisplayTitle: item.week.displayTitle ?? null,
                    videoDuration: item.week.videoDuration ?? null,
                  };
                });

                cards.sort((a, b) => {
                  const order = { upcoming: 0, active: 1, submitted: 2, reviewed: 3, expired: 4 };
                  const oa = order[a.status] ?? 0;
                  const ob = order[b.status] ?? 0;
                  if (oa !== ob) return oa - ob;
                  return (a.assignment.title ?? '').localeCompare(b.assignment.title ?? '');
                });

                console.log(`[Assignments] Mapped ${cards.length} assignments for program ${programId}:`, cards.map(c => ({
                  title: c.assignment.title,
                  subStatus: c.submission?.status,
                  marks: c.submission?.marks,
                  status: c.status
                })));

                this.allCards.set(cards);
                this.isLoadingCards.set(false);
              },
              error: () => { this.isLoadingCards.set(false); },
            });
          },
          error: () => { this.isLoadingCards.set(false); },
        });
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 403) { this.isForbidden.set(true); this.errorMessage.set('You do not have permission to view assignments. This may require backend authorization updates.'); }
        else { this.errorMessage.set('Failed to load program data.'); }
        this.isLoadingCards.set(false);
      },
    });
  }

  public onFileSelected(event: Event, cardKey: string): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      const map = new Map(this.selectedFileMap());
      map.set(cardKey, input.files[0]);
      this.selectedFileMap.set(map);
    }
  }

  public getSelectedFile(cardKey: string): File | null {
    return this.selectedFileMap().get(cardKey) ?? null;
  }

  public submitAssignment(card: AssignmentCardData): void {
    const key = `${card.week.id}-${card.assignment.id}`;
    if (card.isBeforeStart || card.isAfterEnd) return;
    const file = this.getSelectedFile(key);
    if (!file) { this.setSubmitState(key, 'Please select a file.', null); return; }

    const a = card.assignment;
    if (a.fileType) {
      const ext = file.name.split('.').pop()?.toLowerCase();
      const allowedKey = a.fileType.toLowerCase().replace(/^\.+/, '');
      if (ext !== allowedKey) {
        this.setSubmitState(key, `File type .${ext} not allowed. Allowed: .${allowedKey}`, null);
        return;
      }
    }

    this.setSubmitState(key, null, null);
    this.setSubmitting(key, true);
    const empAssignmentId = String(card.submission?.id ?? '');

    if (!empAssignmentId) {
      this.setSubmitting(key, false);
      this.setSubmitState(key, 'No submission record found. Please try refreshing.', null);
      return;
    }

    const progId = String(card.program.id);
    const weekId = String(card.week.id);
    const assignId = String(card.assignment.id);
    this.empAssignmentService.submit(progId, weekId, assignId, empAssignmentId, file).subscribe({
      next: (res) => {
        this.setSubmitting(key, false);
        this.setSubmitState(key, null, 'Submitted successfully!');
        this.clearFile(key);
        this.refreshCard(key, res as {} as EmployeeAssignmentDetailDto);
      },
      error: (err: HttpErrorResponse) => {
        this.setSubmitting(key, false);
        this.setSubmitState(key, err.error?.message || 'Submission failed.', null);
      },
    });
  }

  public downloadSubmissionFile(card: AssignmentCardData): void {
    if (!card.submission) return;
    const fileName = card.submission.fileName || `submission-${card.assignment.id}`;
    const subId = String(card.submission.id ?? card.submission.employeeAssignmentId ?? '');

    if (subId) {
      const progId = String(card.program.id);
      const weekId = String(card.week.id);
      const assignId = String(card.assignment.id);

      this.empAssignmentService.downloadFile(progId, weekId, assignId, subId).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = fileName;
          document.body.appendChild(a);
          a.click();
          a.remove();
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.triggerFallbackDownload(card.submission?.filePath, fileName);
        },
      });
    } else {
      this.triggerFallbackDownload(card.submission?.filePath, fileName);
    }
  }

  private triggerFallbackDownload(filePath?: string | null, fileName: string = 'submission'): void {
    if (!filePath) {
      this.toast.error('Download Error', 'No submission file available for download.');
      return;
    }
    const fileUrl = filePath.startsWith('http')
      ? filePath
      : `${OpenAPI.BASE}${filePath.startsWith('/') ? '' : '/'}${filePath}`;
    const a = document.createElement('a');
    a.href = fileUrl;
    a.download = fileName;
    a.target = '_blank';
    document.body.appendChild(a);
    a.click();
    a.remove();
  }

  public previewSubmissionFile(card: AssignmentCardData): void {
    if (!card.submission) return;
    const subId = String(card.submission.id ?? card.submission.employeeAssignmentId ?? '');

    if (subId) {
      const progId = String(card.program.id);
      const weekId = String(card.week.id);
      const assignId = String(card.assignment.id);

      this.empAssignmentService.downloadFile(progId, weekId, assignId, subId).subscribe({
        next: (blob) => {
          const ext = card.submission?.fileName?.split('.').pop()?.toLowerCase();
          let mimeType = blob.type;
          if (!mimeType || mimeType === 'application/octet-stream') {
            if (ext === 'pdf') mimeType = 'application/pdf';
            else if (ext === 'png') mimeType = 'image/png';
            else if (ext === 'jpg' || ext === 'jpeg') mimeType = 'image/jpeg';
            else if (ext === 'txt') mimeType = 'text/plain';
          }
          const typedBlob = new Blob([blob], { type: mimeType });
          const url = window.URL.createObjectURL(typedBlob);
          window.open(url, '_blank');
        },
        error: () => {
          this.triggerFallbackPreview(card.submission?.filePath);
        },
      });
    } else {
      this.triggerFallbackPreview(card.submission?.filePath);
    }
  }

  private triggerFallbackPreview(filePath?: string | null): void {
    if (!filePath) {
      this.toast.error('Preview Error', 'No submission file path available for preview.');
      return;
    }
    const fileUrl = filePath.startsWith('http')
      ? filePath
      : `${OpenAPI.BASE}${filePath.startsWith('/') ? '' : '/'}${filePath}`;
    window.open(fileUrl, '_blank');
  }

  private computeCardStatus(assignment: AssignmentDto, submission: EmployeeAssignmentDetailDto | null): AssignmentCardData['status'] {
    const now = new Date();
    const startDate = assignment.startDate ? new Date(assignment.startDate) : null;
    const endDate = assignment.endDate ? new Date(assignment.endDate) : null;
    const isBeforeStart = startDate ? startDate > now : false;
    const isAfterEnd = endDate ? endDate < now : false;

    const subStatus = submission?.status?.trim().toLowerCase() ?? '';
    const hasMarks = submission?.marks !== null && submission?.marks !== undefined;
    const hasSubmittedFile = !!(submission?.filePath || submission?.fileName);

    const isReviewedStatus = hasMarks || subStatus === 'reviewed' || subStatus === 'evaluated' || subStatus === 'graded' || subStatus === 'completed';
    const isSubmittedStatus = subStatus === 'submitted' || hasSubmittedFile;

    if (isReviewedStatus) {
      return 'reviewed';
    }
    if (isSubmittedStatus) {
      return 'submitted';
    }
    if (isAfterEnd) {
      return 'expired';
    }
    if (isBeforeStart) {
      return 'upcoming';
    }
    return 'active';
  }

  private refreshCard(key: string, updated: EmployeeAssignmentDetailDto): void {
    this.allCards.update((cards) =>
      cards.map((c) => {
        if (`${c.week.id}-${c.assignment.id}` === key) {
          const status = this.computeCardStatus(c.assignment, updated);
          return { ...c, submission: updated, status, canSubmit: false };
        }
        return c;
      })
    );
  }

  private setSubmitState(key: string, error: string | null, success: string | null): void {
    const map = new Map(this.submitStates());
    map.set(key, { submitting: map.get(key)?.submitting ?? false, error, success });
    this.submitStates.set(map);
  }

  private setSubmitting(key: string, submitting: boolean): void {
    const map = new Map(this.submitStates());
    const current = map.get(key);
    map.set(key, { submitting, error: current?.error ?? null, success: current?.success ?? null });
    this.submitStates.set(map);
  }

  private clearFile(key: string): void {
    const map = new Map(this.selectedFileMap());
    map.delete(key);
    this.selectedFileMap.set(map);
  }

  public getSubmitState(card: AssignmentCardData): { submitting: boolean; error: string | null; success: string | null } {
    const key = `${card.week.id}-${card.assignment.id}`;
    return this.submitStates().get(key) ?? { submitting: false, error: null, success: null };
  }

  public showVideo(card: AssignmentCardData): void {
    const progId = String(card.program.id);
    const weekId = String(card.week.id);
    const data: VideoPlayerData = {
      programId: progId,
      weekId,
      title: (card.videoDisplayTitle ?? card.week.topic) || `Week ${card.week.weekNumber} Session`,
      duration: card.videoDuration,
      weekNumber: card.week.weekNumber,
    };
    this.monolithService.getProgramProgramWeekVideo(Number(progId), Number(weekId)).subscribe({
      next: (blob: Blob) => {
        const url = URL.createObjectURL(blob);
        const safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        this.videoPlayerModal.open(data, safeUrl);
      },
      error: () => {
        this.errorMessage.set('Failed to load video. Please try again.');
      },
    });
  }

  public timeProgress(card: AssignmentCardData): number {
    if (card.status !== 'active') return 0;
    const start = card.assignment.startDate ? new Date(card.assignment.startDate).getTime() : null;
    const end = card.assignment.endDate ? new Date(card.assignment.endDate).getTime() : null;
    if (!start || !end || end <= start) return 0;
    const now = Date.now();
    if (now <= start) return 0;
    if (now >= end) return 100;
    return Math.round(((now - start) / (end - start)) * 100);
  }

  public daysUntil(card: AssignmentCardData): number | null {
    const ref = card.status === 'upcoming' ? card.assignment.startDate : card.assignment.endDate;
    if (!ref) return null;
    const diff = new Date(ref).getTime() - Date.now();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  }

  public daysLabel(days: number): string {
    if (days === 0) return 'Last day';
    if (days === 1) return '1 day left';
    return `${days} days left`;
  }

  public statusDisplay(status: AssignmentCardData['status']): string {
    switch (status) {
      case 'upcoming': return 'Upcoming';
      case 'active': return 'Active';
      case 'submitted': return 'Submitted';
      case 'reviewed': return 'Graded';
      case 'expired': return 'Expired';
      default: return 'Unknown';
    }
  }

  public statusSeverity(status: AssignmentCardData['status']): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' | undefined {
    switch (status) {
      case 'active': return 'warning';
      case 'submitted': return 'info';
      case 'reviewed': return 'success';
      case 'upcoming': return 'secondary';
      case 'expired': return 'danger';
      default: return undefined;
    }
  }

  public getAssignmentLink(card: AssignmentCardData): string[] {
    return ['/employee/programs', this.selectedProgramId(), 'weeks', card.week.id, 'assignments', String(card.assignment.id)];
  }

  public getWeekLink(card: AssignmentCardData): string[] {
    return ['/employee/programs', this.selectedProgramId(), 'weeks', card.week.id];
  }
}
