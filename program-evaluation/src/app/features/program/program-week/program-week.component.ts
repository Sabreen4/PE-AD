import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { ProgramWeekService } from '@features/program/services/program-week.service';
import { ProgramWeek } from '@features/program/models/program-week.model';
import { TableColumnConfig, FormFieldConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-program-week',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './program-week.component.html',
})
export class ProgramWeekComponent implements OnInit {
  private weekService = inject(ProgramWeekService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  loading = signal(true);
  weeks = signal<ProgramWeek[]>([]);
  programId = '';

  columns: TableColumnConfig<ProgramWeek>[] = [
    { key: 'weekNumber', label: 'Week #' },
    { key: 'topic', label: 'Topic', formatter: (r: ProgramWeek) => r.topic || '—' },
    { key: 'startDate', label: 'Start', formatter: (r: ProgramWeek) => new Date(r.startDate).toLocaleDateString() },
    { key: 'endDate', label: 'End', formatter: (r: ProgramWeek) => new Date(r.endDate).toLocaleDateString() }
  ];

  private readonly fields: FormFieldConfig[] = [
    { key: 'weekNumber', label: 'Week number', type: 'number', required: true },
    { key: 'topic', label: 'Topic', type: 'text' },
    { key: 'description', label: 'Description', type: 'textarea' },
    { key: 'startDate', label: 'Start date', type: 'date', required: true },
    { key: 'endDate', label: 'End date', type: 'date', required: true }
  ];

  ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('programId') || this.route.parent?.snapshot.paramMap.get('id') || '';
    if (!this.programId) { this.loading.set(false); return; }
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.weekService.listByProgram(this.programId, { pageSize: 100 }).subscribe({
      next: (result) => { this.weeks.set(result.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: 'Add week', fields: this.fields, submitText: 'Create' }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.weekService.create(this.programId, value as Partial<ProgramWeek>).subscribe(() => {
        this.load();
      });
    });
  }

  openEdit(week: ProgramWeek): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: 'Edit week', fields: this.fields, model: week, submitText: 'Save' }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.weekService.update(this.programId, week.id, value as Partial<ProgramWeek>).subscribe(() => {
        this.load();
      });
    });
  }

  remove(week: ProgramWeek): void {
    this.weekService.delete(this.programId, week.id).subscribe(() => {
      this.load();
    });
  }
}