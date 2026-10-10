import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-program-weeks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './program-weeks.component.html',
  styleUrl: './program-weeks.component.scss'
})
export class ProgramWeeksComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);

  public readonly programs = signal<Program[]>([]);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly selectedProgramId = signal<string>('');
  public readonly isLoading = signal(false);
  public readonly isLoadingWeeks = signal(false);
  public readonly errorMessage = signal<string | null>(null);

  public ngOnInit(): void {
    this.loadPrograms();
  }

  public onProgramChange(programId: string): void {
    this.selectedProgramId.set(programId);
    if (programId) this.loadWeeks(programId);
    else this.weeks.set([]);
  }

  public isCurrentWeek(week: ProgramWeek): boolean {
    const now = new Date();
    return new Date(week.startDate) <= now && new Date(week.endDate) >= now;
  }

  public isCompletedWeek(week: ProgramWeek): boolean {
    return new Date(week.endDate) < new Date();
  }

  public isUpcomingWeek(week: ProgramWeek): boolean {
    return new Date(week.startDate) > new Date();
  }

  private loadPrograms(): void {
    this.isLoading.set(true);
    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => { this.programs.set(res.items); this.isLoading.set(false); },
      error: () => { this.errorMessage.set('Failed to load programs.'); this.isLoading.set(false); },
    });
  }

  private loadWeeks(programId: string): void {
    this.isLoadingWeeks.set(true);
    this.programWeekService.listByProgram(programId, { page: 1, limit: 100 }).subscribe({
      next: (res) => { this.weeks.set(res.items); this.isLoadingWeeks.set(false); },
      error: () => { this.errorMessage.set('Failed to load weeks.'); this.isLoadingWeeks.set(false); },
    });
  }
}
