import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { ProgramService } from '@features/program/services/program.service';
import { Program } from '@features/program/models/program.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-program-details',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTabsModule, LoadingSpinnerComponent, BackNavComponent],
  templateUrl: './program-details.component.html',
  styleUrls: ['./program-details.component.scss']
})
export class ProgramDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private programService = inject(ProgramService);

  loading = signal(true);
  program = signal<Program | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.programService.getById(id).subscribe({
      next: (p) => { this.program.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}