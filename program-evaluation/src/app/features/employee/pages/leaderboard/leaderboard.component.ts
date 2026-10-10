import { Component, DestroyRef, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LeaderboardService } from '@features/evaluation/services/leaderboard.service';
import { ProgramService } from '@features/program/services/program.service';
import { LeaderboardEntry } from '@features/evaluation/models/leaderboard.model';
import { Program } from '@features/program/models/program.model';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';

import { MatIconModule } from '@angular/material/icon';
import { DropdownModule } from 'primeng/dropdown';

@Component({
  selector: 'app-employee-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, BackNavComponent, DropdownModule, MatIconModule],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.scss'
})
export class EmployeeLeaderboardComponent implements OnInit {
  private readonly leaderboardService: LeaderboardService = inject(LeaderboardService);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  public readonly userId = computed(() => this.authState.user()?.userId ?? '');
  public readonly programs = signal<Program[]>([]);
  public readonly entries = signal<LeaderboardEntry[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly isLoading = signal(false);
  public readonly isLoadingPrograms = signal(false);
  public readonly searchQuery = signal<string>('');

  public readonly sortedEntries = computed(() =>
    [...this.entries()].sort((a, b) => a.rank - b.rank)
  );

  public readonly topThree = computed(() =>
    this.sortedEntries().slice(0, 3)
  );

  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(25);
  public readonly totalRecords = computed(() => this.entries().length);

  public readonly paginatedEntries = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.entries().slice(start, start + this.pageSize());
  });

  public readonly myEntry = computed(() =>
    this.entries().find((e) => e.employeeId === this.userId())
  );

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

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }

  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
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

  private loadPrograms(): void {
    this.isLoadingPrograms.set(true);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => { this.programs.set(res.items); this.isLoadingPrograms.set(false); },
      error: () => { this.isLoadingPrograms.set(false); },
    });
  }

  private loadLeaderboard(programId: string, search?: string): void {
    this.isLoading.set(true);
    this.leaderboardService.list(programId, search || undefined).subscribe({
      next: (res) => { this.entries.set(res); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }
}
