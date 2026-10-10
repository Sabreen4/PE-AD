import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { ProgramService } from '@features/program/services/program.service';
import { Program } from '@features/program/models/program.model';
import { ToastService } from '@shared/toast/toast.service';

@Component({
  selector: 'app-employee-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, BackNavComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class EmployeeDashboardComponent implements OnInit {
  private readonly authState = inject(AuthenticationStateService);
  private readonly programService = inject(ProgramService);
  private readonly toast = inject(ToastService);

  public readonly user = this.authState.user;
  public readonly userId = computed(() => this.authState.user()?.userId ?? '');

  public readonly programs = signal<Program[]>([]);
  public readonly isLoading = signal(false);

  public readonly totalPrograms = computed(() => this.programs().length);

  public ngOnInit(): void {
    this.loadPrograms();
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
        this.toast.error('Dashboard', 'Failed to load programs.');
      },
    });
  }
}
