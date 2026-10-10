import { Component, inject, signal, OnInit, computed, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconModule } from '@angular/material/icon';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { AssignmentService } from '@features/program/services/assignment.service';
import { EmployeeAssignmentService } from '@features/program/services/employee-assignment.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { ProgramService as MonolithProgramService } from '@features/program/services/ProgramService';
import { EvaluationService } from '@features/evaluation/services/EvaluationService';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { EnrollmentDetailDto } from '@features/program/models/EnrollmentDetailDto';
import { AssignmentDetailDto } from '@features/program/models/AssignmentDetailDto';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { VideoPlayerModalComponent, VideoPlayerData } from '../../components/video-player-modal/video-player-modal.component';
import { DropdownModule } from 'primeng/dropdown';

interface AssignmentInfo {
  id: number;
  title: string;
  startDate: string;
  endDate: string;
  available: boolean;
  notStarted: boolean;
  closed: boolean;
}

interface WeekTimelineData {
  week: ProgramWeek;
  videoAvailable: boolean;
  videoDuration: string | null;
  videoName: string | null;
  videoDisplayTitle: string | null;
  assignments: AssignmentInfo[];
  employeeSubmission: { status: string; marks: number | null } | null;
  attendance: { marked: boolean; percentage: number };
  evaluation: { engagementScore: number | null; applicationScore: number | null; weeklyAverage: number | null } | null;
  completed: boolean;
  current: boolean;
  upcoming: boolean;
}

@Component({
  selector: 'app-progress-timeline',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, VideoPlayerModalComponent, BackNavComponent, DropdownModule],
  templateUrl: './progress-timeline.component.html',
  styleUrl: './progress-timeline.component.scss'
})
export class ProgressTimelineComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly assignmentService: AssignmentService = inject(AssignmentService);
  private readonly empAssignmentService: EmployeeAssignmentService = inject(EmployeeAssignmentService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly monolithService: MonolithProgramService = inject(MonolithProgramService);
  private readonly evaluationService: EvaluationService = inject(EvaluationService);
  private readonly sanitizer: DomSanitizer = inject(DomSanitizer);

  @ViewChild('videoPlayer') videoPlayerModal!: VideoPlayerModalComponent;

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<WeekTimelineData[]>([]);
  public readonly selectedProgram = signal<Program | null>(null);
  public readonly enrollment = signal<EnrollmentDetailDto | null>(null);
  public readonly selectedProgramId = signal<string>('');
  public readonly isLoading = signal(false);
  public readonly isLoadingDetails = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly errorType = signal<'network' | 'notfound' | 'forbidden' | 'generic' | null>(null);
  public readonly selectedWeekId = signal<string | null>(null);
  public readonly forbiddenDetails = signal<string[]>([]);

  public readonly overallProgress = computed(() => {
    const list = this.weeks();
    if (list.length === 0) return 0;
    return Math.round((list.filter((w) => w.completed).length / list.length) * 100);
  });

  public readonly totalWeeks = computed(() => this.weeks().length);
  public readonly currentWeekNumber = computed(() => {
    const cw = this.weeks().find((w) => w.current);
    return cw?.week.weekNumber ?? this.enrollment()?.currentWeek ?? 0;
  });
  public readonly completedWeeksCount = computed(() => this.weeks().filter((w) => w.completed).length);

  public readonly avgAttendance = computed(() => {
    const list = this.weeks();
    if (list.length === 0) return 0;
    return Math.round(list.reduce((s, w) => s + w.attendance.percentage, 0) / list.length);
  });

  public readonly avgAssignmentScore = computed(() => {
    const scored = this.weeks().filter((w) => w.employeeSubmission?.marks != null);
    if (scored.length === 0) return null;
    return (scored.reduce((sum, w) => sum + (w.employeeSubmission!.marks ?? 0), 0) / scored.length).toFixed(1);
  });

  public readonly selectedWeekData = computed(() => {
    const id = this.selectedWeekId();
    if (!id) return null;
    return this.weeks().find((w) => w.week.id === id) ?? null;
  });

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public toggleWeek(weekId: string): void {
    this.selectedWeekId.update((current) => (current === weekId ? null : weekId));
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.selectedWeekId.set(null);
    if (programId) this.loadProgramTimeline(programId);
    else { this.weeks.set([]); this.selectedProgram.set(null); this.enrollment.set(null); }
  }

  public showVideo(programId: string, weekId: string, wd: WeekTimelineData): void {
    const data: VideoPlayerData = {
      programId,
      weekId,
      title: (wd.videoDisplayTitle ?? wd.videoName) || `Week ${wd.week.weekNumber} Session`,
      duration: wd.videoDuration,
      weekNumber: wd.week.weekNumber,
    };
    this.monolithService.getProgramProgramWeekVideo(Number(programId), Number(weekId)).subscribe({
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

  public isAssignmentAvailable(a: AssignmentInfo): boolean { return a.available; }
  public isAssignmentNotStarted(a: AssignmentInfo): boolean { return a.notStarted; }
  public isAssignmentClosed(a: AssignmentInfo): boolean { return a.closed; }

  public submissionLabel(sub: { status: string; marks: number | null } | null): string {
    if (!sub) return 'No Submission';
    const s = sub.status?.toLowerCase() ?? '';
    if (s === 'evaluated' || s === 'reviewed') return sub.marks != null ? `${sub.marks}/100 (Reviewed)` : 'Reviewed';
    if (s === 'submitted') return 'Submitted';
    return 'Not Started';
  }

  public submissionBadgeLabel(sub: { status: string; marks: number | null } | null): string {
    if (!sub) return 'Not Started';
    const s = sub.status?.toLowerCase() ?? '';
    if (s === 'evaluated' || s === 'reviewed') return 'Reviewed';
    if (s === 'submitted') return 'Pending Review';
    return 'Not Started';
  }

  public getEvalLabel(score: number): string {
    if (score >= 3) return 'Excellent';
    if (score >= 2) return 'Good';
    if (score >= 1) return 'Fair';
    return 'Needs Improvement';
  }

  public daysRemaining(endDate: string): number {
    return Math.max(0, Math.ceil((new Date(endDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24)));
  }

  public daysLabel(days: number): string {
    if (days === 0) return 'Last day';
    if (days === 1) return '1 Day Left';
    return `${days} Days Left`;
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => {
        const allPrograms = res.items;
        if (allPrograms.length === 0) { this.programs.set([]); this.isLoading.set(false); return; }
        const enrollmentCalls = allPrograms.map((p) =>
          this.enrollmentService.getMyEnrollment(p.id).pipe(
            map((enr) => ({ program: p, enrollment: enr })),
            catchError(() => of({ program: p, enrollment: null }))
          )
        );
        forkJoin(enrollmentCalls).subscribe({
          next: (results) => {
            const enrolled = results.filter((r) => r.enrollment != null);
            if (enrolled.length === 0) { this.programs.set([]); this.isLoading.set(false); return; }
            this.programs.set(enrolled.map((r) => r.program));
            this.isLoading.set(false);
            if (enrolled.length > 0) {
              this.selectedProgramId.set(enrolled[0].program.id);
              this.enrollment.set(enrolled[0].enrollment);
              this.loadProgramTimeline(enrolled[0].program.id);
            }
          },
          error: () => this.isLoading.set(false),
        });
      },
      error: () => this.isLoading.set(false),
    });
  }

  private loadProgramTimeline(programId: string): void {
    this.isLoadingDetails.set(true);
    this.errorMessage.set(null);
    this.errorType.set(null);
    forkJoin({
      program: this.programService.getById(programId),
      weeks: this.programWeekService.listByProgram(programId),
      enrollment: this.enrollmentService.getMyEnrollment(programId),
    }).subscribe({
      next: (res) => {
        const prog = res.program;
        this.selectedProgram.set({ ...(prog ?? ({} as Program)), id: programId });
        this.enrollment.set(res.enrollment);
        const weeks = res.weeks.items;
        if (weeks.length === 0) { this.weeks.set([]); this.isLoadingDetails.set(false); return; }
        this.enrichWeeks(programId, weeks, res.enrollment ?? undefined);
      },
      error: (err: HttpErrorResponse) => { this.handleError(err); this.isLoadingDetails.set(false); },
    });
  }

  private handleError(err: HttpErrorResponse): void {
    if (err.status === 403) { this.errorMessage.set('You do not have permission to view this program timeline.'); this.errorType.set('forbidden'); }
    else if (err.status === 404) { this.errorMessage.set('Program data not found. It may have been removed.'); this.errorType.set('notfound'); }
    else if (err.status === 401) { this.errorMessage.set('Your session has expired. Please log in again.'); this.errorType.set('network'); }
    else { this.errorMessage.set('Failed to load program data. Please try again later.'); this.errorType.set('generic'); }
  }

  private enrichWeeks(programId: string, weeks: ProgramWeek[], enrollment: EnrollmentDetailDto | null | undefined): void {
    const enrollmentId = enrollment?.id ?? null;
    this.forbiddenDetails.set([]);

    const videoCalls = weeks.map((w) =>
      this.monolithService.getProgramProgramWeekVideoDetails(Number(programId), Number(w.id)).pipe(
        map((meta) => ({ weekId: w.id, available: meta?.videoName != null, videoName: meta?.videoName ?? null, displayTitle: meta?.displayTitle ?? null, videoDuration: meta?.videoDuration ?? null })),
        catchError(() => of({ weekId: w.id, available: false, videoName: null, displayTitle: null, videoDuration: null }))
      )
    );

    const assignmentCalls = weeks.map((w) =>
      this.assignmentService.listByWeek(programId, w.id).pipe(
        map((items) => ({ weekId: w.id, items: items ?? [] })),
        catchError(() => of({ weekId: w.id, items: [] }))
      )
    );

    const attendanceCalls = enrollmentId
      ? weeks.map((w) =>
          this.evaluationService.getEnrollmentAttendance(Number(enrollmentId), 1, 100).pipe(
            map((res) => {
              const weekRecords = (res?.data ?? []).filter((r) => String(r.programWeekId) === w.id);
              return { weekId: w.id, marked: weekRecords.length > 0, percentage: weekRecords.length > 0 ? 100 : 0 };
            }),
            catchError((err) => {
              if (err?.status === 403) this.forbiddenDetails.update((d) => [...d, 'Attendance: requires backend authorization for employee self-access.']);
              return of({ weekId: w.id, marked: false, percentage: 0 });
            })
          )
        )
      : weeks.map((w) => of({ weekId: w.id, marked: false, percentage: 0 }));

    const evaluationCalls = enrollmentId
      ? weeks.map((w) =>
          this.evaluationService.getEnrollmentWeeklyEvaluation(Number(enrollmentId), 1, 100).pipe(
            map((res) => {
              const weekEval = (res?.data ?? []).find((e) => String(e.programWeekId) === w.id);
              return {
                weekId: w.id,
                evaluation: weekEval ? { engagementScore: weekEval.engagementScore ?? null, applicationScore: weekEval.applicationScore ?? null, weeklyAverage: weekEval.weeklyAverage ?? null } : null,
              };
            }),
            catchError((err) => {
              if (err?.status === 403) this.forbiddenDetails.update((d) => [...d, 'Evaluation: requires backend authorization for employee self-access.']);
              return of({ weekId: w.id, evaluation: null });
            })
          )
        )
      : weeks.map((w) => of({ weekId: w.id, evaluation: null }));

    forkJoin({ videos: forkJoin(videoCalls), assignments: forkJoin(assignmentCalls), attendances: forkJoin(attendanceCalls), evaluations: forkJoin(evaluationCalls) })
      .subscribe({
        next: (res) => {
          const now = new Date();
          const submissionCalls = res.assignments.flatMap((ar) =>
            ar.items.length > 0
              ? ar.items.map((a) =>
                  this.empAssignmentService.getMyAssignment(programId, ar.weekId, String(a.id)).pipe(
                    map((sub) => ({ weekId: ar.weekId, assignmentId: String(a.id), status: sub?.status ?? null, marks: sub?.marks ?? null })),
                    catchError(() => of({ weekId: ar.weekId, assignmentId: String(a.id), status: null, marks: null }))
                  )
                )
              : [of({ weekId: ar.weekId, assignmentId: '', status: null, marks: null })]
          );
          if (submissionCalls.length === 0) { this.buildTimelineData(weeks, now, res, []); return; }
          forkJoin(submissionCalls).subscribe({
            next: (subResults) => this.buildTimelineData(weeks, now, res, subResults),
            error: () => this.buildTimelineData(weeks, now, res, []),
          });
        },
        error: () => { this.errorMessage.set('Failed to load week details.'); this.errorType.set('generic'); this.isLoadingDetails.set(false); },
      });
  }

  private buildTimelineData(
    weeks: ProgramWeek[],
    now: Date,
    enrichment: { videos: { weekId: string; available: boolean; videoName: string | null; displayTitle: string | null; videoDuration: string | null }[]; assignments: { weekId: string; items: AssignmentDetailDto[] }[]; attendances: { weekId: string; marked: boolean; percentage: number }[]; evaluations: { weekId: string; evaluation: { engagementScore: number | null; applicationScore: number | null; weeklyAverage: number | null } | null }[] },
    submissions: { weekId: string; status: string | null; marks: number | null }[]
  ): void {
    const data: WeekTimelineData[] = weeks.map((w) => {
      const completed = new Date(w.endDate) < now;
      const current = new Date(w.startDate) <= now && new Date(w.endDate) >= now;
      const upcoming = new Date(w.startDate) > now;
      const weekSubs = submissions.filter((s) => s.weekId === w.id && s.status != null);
      const latestSub = weekSubs.length > 0 ? { status: weekSubs[0].status ?? 'pending', marks: weekSubs[0].marks } : null;
      const videoInfo = enrichment.videos.find((v) => v.weekId === w.id);

      const assignmentItems = enrichment.assignments.find((a) => a.weekId === w.id)?.items ?? [];
      const assignments: AssignmentInfo[] = assignmentItems.map((a) => {
        const aStart = a.startDate ? new Date(a.startDate) : null;
        const aEnd = a.endDate ? new Date(a.endDate) : null;
        return {
          id: a.id ?? 0,
          title: a.title ?? '',
          startDate: a.startDate ?? '',
          endDate: a.endDate ?? '',
          available: aStart != null && aEnd != null && now >= aStart && now <= aEnd,
          notStarted: aStart != null && now < aStart,
          closed: aEnd != null && now > aEnd,
        };
      });

      return {
        week: w,
        videoAvailable: videoInfo?.available ?? false,
        videoName: videoInfo?.videoName ?? null,
        videoDisplayTitle: videoInfo?.displayTitle ?? null,
        videoDuration: videoInfo?.videoDuration ?? null,
        assignments,
        employeeSubmission: latestSub,
        attendance: enrichment.attendances.find((a) => a.weekId === w.id) ?? { marked: false, percentage: 0 },
        evaluation: enrichment.evaluations.find((e) => e.weekId === w.id)?.evaluation ?? null,
        completed, current, upcoming,
      };
    });
    this.weeks.set(data);
    this.isLoadingDetails.set(false);

    const cur = data.find((w) => w.current);
    if (cur) this.selectedWeekId.set(cur.week.id);
  }
}
