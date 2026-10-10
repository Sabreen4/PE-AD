import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProgramService } from '@features/program/services/program.service';
import { Program } from '@features/program/models/program.model';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-employee-programs',
  standalone: true,
  imports: [CommonModule, RouterLink, BackNavComponent],
  templateUrl: './programs.component.html',
  styleUrl: './programs.component.scss'
})
export class EmployeeProgramsComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);

  public readonly programs = signal<Program[]>([]);
  public readonly isLoading = signal(false);
  public readonly errorMessage = signal<string | null>(null);

  public ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.programService.list({ page: 1, limit: 100 }).subscribe({
      next: (res) => {
        this.programs.set(res.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load programs.');
        this.isLoading.set(false);
      },
    });
  }
}
