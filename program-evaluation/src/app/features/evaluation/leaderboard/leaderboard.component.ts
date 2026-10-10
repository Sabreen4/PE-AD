import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { LeaderboardService } from '@features/evaluation/services/leaderboard.service';
import { ProgramService } from '@features/program/services/program.service';
import { LeaderboardEntry } from '@features/evaluation/models/leaderboard.model';
import { Program } from '@features/program/models/program.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatButtonModule, MatSelectModule, LoadingSpinnerComponent, EmptyStateComponent, BackNavComponent],
  templateUrl: './leaderboard.component.html',
  styleUrls: ['./leaderboard.component.scss'],
})
export class LeaderboardComponent implements OnInit {
  private readonly leaderboardService = inject(LeaderboardService);
  private readonly programService = inject(ProgramService);

  loading = signal(false);
  entries = signal<LeaderboardEntry[]>([]);
  programs = signal<Program[]>([]);
  selectedProgramId = signal<string>('');
  displayedColumns = ['rank', 'employeeName', 'overallScore', 'percentile'];

  ngOnInit(): void {
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => this.programs.set(res.items),
    });
  }

  onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    if (programId) this.load();
  }

  load(): void {
    const pid = this.selectedProgramId();
    if (!pid) return;
    this.loading.set(true);
    this.leaderboardService.list(pid).subscribe({
      next: (result) => {
        this.entries.set(result);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
