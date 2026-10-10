import { Component, DestroyRef, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProgramService } from '@features/program/services/program.service';
import { LeaderboardService } from '@features/evaluation/services/leaderboard.service';
import { Program } from '@features/program/models/program.model';
import { LeaderboardEntry } from '@features/evaluation/models/leaderboard.model';
import { ToastService } from '@shared/toast/toast.service';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

interface LeaderboardRow extends LeaderboardEntry {
  initials: string;
}

@Component({
  selector: 'app-evaluator-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, PaginationComponent, BackNavComponent],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.scss',
})
export class EvaluatorLeaderboardComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly leaderboardService: LeaderboardService = inject(LeaderboardService);
  private readonly toastService: ToastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  public readonly programs = signal<Program[]>([]);
  public readonly entries = signal<LeaderboardRow[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly isLoading = signal(false);
  public readonly searchQuery = signal<string>('');
  public readonly isRecalculating = signal(false);
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(25);
  public readonly totalRecords = computed(() => this.entries().length);
  public readonly paginatedEntries = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.entries().slice(start, start + this.pageSize());
  });

  public readonly sortedEntries = computed<LeaderboardRow[]>(() =>
    [...this.entries()].sort((a, b) => a.rank - b.rank)
  );

  public readonly topThree = computed(() => this.sortedEntries().slice(0, 3));

  constructor() {
    this.search$.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value: string) => {
      this.searchQuery.set(value);
      this.currentPage.set(1);
      const pid = this.selectedProgramId();
      if (pid) this.loadLeaderboard(pid, value);
    });
  }

  public readonly averageScore = computed(() => {
    const all = this.entries();
    if (all.length === 0) return 0;
    return all.reduce((sum, e) => sum + e.overallScore, 0) / all.length;
  });

  public readonly topPerformers = computed(() =>
    this.entries().filter((e) => e.overallScore >= 80)
  );

  public readonly topPerformersPercent = computed(() =>
    this.entries().length > 0
      ? (this.topPerformers().length / this.entries().length * 100).toFixed(0)
      : '0'
  );

  public readonly totalEntries = computed(() => this.entries().length);

  public readonly flagAlerts = computed(() =>
    this.entries().filter((e) => e.overallScore < 50).length
  );

  public readonly selectedProgramName = computed(() => {
    const id = this.selectedProgramId();
    if (!id) return '';
    return this.programs().find((p) => p.id === id)?.name || '';
  });

  public readonly selectedProgramDuration = computed(() => {
    const id = this.selectedProgramId();
    const p = this.programs().find((pr) => pr.id === id);
    return p?.durationWeeks ?? p?.totalWeeks ?? '';
  });

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    this.searchQuery.set('');
    this.currentPage.set(1);
    if (programId) this.loadLeaderboard(programId);
    else this.entries.set([]);
  }

  public onSearchInput(event: Event): void {
    this.search$.next((event.target as HTMLInputElement).value);
  }

  public clearSearch(input: HTMLInputElement): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    input.value = '';
    const pid = this.selectedProgramId();
    if (pid) this.loadLeaderboard(pid);
  }

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }

  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  public recalculate(): void {
    const programId = this.selectedProgramId();
    if (!programId) return;
    this.isRecalculating.set(true);
    this.leaderboardService.recalculate(programId).subscribe({
      next: (res) => {
        this.entries.set(
          res.map((e) => ({
            ...e,
            initials: this.getInitials(e.employeeName),
          }))
        );
        this.isRecalculating.set(false);
        this.toastService.success('Leaderboard', 'Leaderboard recalculated successfully');
      },
      error: () => {
        this.isRecalculating.set(false);
        this.toastService.error('Leaderboard', 'Failed to recalculate leaderboard');
      },
    });
  }

  public exportCsv(): void {
    const rows = this.entries();
    if (rows.length === 0) return;
    const headers = [
      'Rank',
      'Employee',
      'Overall Score',
      'Percentile',
    ];
    const lines = rows.map((r: LeaderboardRow) => [
      r.rank.toString(),
      r.employeeName,
      r.overallScore.toFixed(1),
      r.percentile.toFixed(1),
    ]);
    const csv = [headers.join(','), ...lines.map((l: string[]) => l.join(','))].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `leaderboard-${this.selectedProgramId()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => {
        this.programs.set(res.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  private loadLeaderboard(programId: string, search?: string): void {
    this.isLoading.set(true);
    this.leaderboardService.list(programId, search || undefined).subscribe({
      next: (res) => {
        this.entries.set(
          res.map((e) => ({
            ...e,
            initials: this.getInitials(e.employeeName),
          }))
        );
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  private getInitials(name: string): string {
    const parts = name.trim().split(/\s+/);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }
}
