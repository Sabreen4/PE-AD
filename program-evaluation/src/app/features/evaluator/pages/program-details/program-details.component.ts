import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProgramService } from '@features/program/services/program.service';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { Program } from '@features/program/models/program.model';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { Enrollment } from '@features/program/models/enrollment.model';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-program-details',
  standalone: true,
  imports: [CommonModule, RouterLink, BackNavComponent],
  templateUrl: './program-details.component.html',
  styleUrl: './program-details.component.scss'
})
export class ProgramDetailsComponent implements OnInit {
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly programWeekService: ProgramWeekService = inject(ProgramWeekService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);

  public readonly program = signal<Program | null>(null);
  public readonly weeks = signal<ProgramWeek[]>([]);
  public readonly enrollments = signal<Enrollment[]>([]);
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);

  public programId: string = '';

  public ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.programId) {
      this.errorMessage.set('Program ID not found.');
      return;
    }
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.programService.getById(this.programId).subscribe({
      next: (prog) => {
        this.program.set(prog);
        this.loadWeeks();
        this.loadEnrollments();
      },
      error: () => {
        this.errorMessage.set('Failed to load program details.');
        this.isLoading.set(false);
      },
    });
  }

  private loadWeeks(): void {
    this.programWeekService.listByProgram(this.programId).subscribe({
      next: (res) => { this.weeks.set(res.items.sort((a, b) => a.weekNumber - b.weekNumber)); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); },
    });
  }

  private loadEnrollments(): void {
    this.enrollmentService.listByProgram(this.programId).subscribe({
      next: (res) => { this.enrollments.set(res.items); },
      error: () => {},
    });
  }

  public goBack(): void {
    this.router.navigate(['/evaluator/programs']);
  }

  public get configuredWeeks(): number {
    return this.weeks().length;
  }

  public get durationWeeks(): number {
    return this.program()?.durationWeeks ?? this.program()?.totalWeeks ?? 0;
  }
}
