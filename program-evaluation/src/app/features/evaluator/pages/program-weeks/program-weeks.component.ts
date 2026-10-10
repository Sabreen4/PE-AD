import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { Program } from '@features/program/models/program.model';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-program-weeks',
  standalone: true,
  imports: [CommonModule, RouterLink, BackNavComponent],
  templateUrl: './program-weeks.component.html',
  styleUrl: './program-weeks.component.scss'
})
export class ProgramWeeksComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);

  public readonly program = signal<Program | null>(null);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);

  public programId: string = '';

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.programId) { this.errorMessage.set('Program ID not found.'); return; }
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.programService.getById(this.programId).subscribe({
      next: (prog) => {
        this.program.set(prog);
        this.loadWeeks();
      },
      error: () => { this.errorMessage.set('Failed to load program.'); this.isLoading.set(false); },
    });
  }

  private loadWeeks(): void {
    this.programWeekService.listByProgram(this.programId).subscribe({
      next: (res) => { this.weeks.set(res.items.sort((a, b) => a.weekNumber - b.weekNumber)); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  public goBack(): void { this.router.navigate(['/evaluator/programs', this.programId]); }

  public get durationWeeks(): number {
    return this.program()?.durationWeeks ?? this.program()?.totalWeeks ?? 0;
  }
}
