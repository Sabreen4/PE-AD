import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { ProgramActivityService } from '@features/program/services/program-activity.service';
import { ProgramActivity } from '@features/program/models/program-activity.model';
import { TableColumnConfig, FormFieldConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-program-activity',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './program-activity.component.html',
})
export class ProgramActivityComponent implements OnInit {
  private activityService = inject(ProgramActivityService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  loading = signal(true);
  activities = signal<ProgramActivity[]>([]);

  columns: TableColumnConfig<ProgramActivity>[] = [
    { key: 'name', label: 'Activity' },
    { key: 'activityType', label: 'Type' },
    { key: 'dueDate', label: 'Due date', formatter: (r: ProgramActivity) => r.dueDate ? new Date(r.dueDate).toLocaleDateString() : '—' },
    { key: 'maxScore', label: 'Max score', formatter: (r: ProgramActivity) => r.maxScore != null ? String(r.maxScore) : '—' },
    { key: 'isMandatory', label: 'Mandatory', formatter: (r: ProgramActivity) => r.isMandatory ? 'Yes' : 'No' }
  ];

  private readonly fields: FormFieldConfig[] = [
    { key: 'name', label: 'Activity name', type: 'text', required: true },
    { key: 'description', label: 'Description', type: 'textarea' },
    { key: 'activityType', label: 'Type', type: 'text', required: true },
    { key: 'dueDate', label: 'Due date', type: 'date' },
    { key: 'maxScore', label: 'Max score', type: 'number' },
    { key: 'isMandatory', label: 'Mandatory', type: 'checkbox' }
  ];

  ngOnInit(): void {
    const weekId = this.route.snapshot.paramMap.get('weekId');
    if (!weekId) return;
    this.load(weekId);
  }

  load(weekId: string): void {
    this.loading.set(true);
    this.activityService.listByWeek(weekId, { pageSize: 100 }).subscribe({
      next: (result) => { this.activities.set(result.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    const weekId = this.route.snapshot.paramMap.get('weekId');
    if (!weekId) return;
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: 'Add activity', fields: this.fields, submitText: 'Create' }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.activityService.create(weekId, value as Partial<ProgramActivity>).subscribe(() => {
        this.load(weekId);
      });
    });
  }

  openEdit(activity: ProgramActivity): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: 'Edit activity', fields: this.fields, model: activity, submitText: 'Save' }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.activityService.update(activity.id, value as Partial<ProgramActivity>).subscribe(() => {
        this.load(activity.programWeekId);
      });
    });
  }

  remove(activity: ProgramActivity): void {
    this.activityService.delete(activity.id).subscribe(() => {
      this.load(activity.programWeekId);
    });
  }
}