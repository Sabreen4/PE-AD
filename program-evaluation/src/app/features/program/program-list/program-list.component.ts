import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { ProgramService } from '@features/program/services/program.service';
import { Program } from '@features/program/models/program.model';
import { TableColumnConfig, FormFieldConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-program-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './program-list.component.html',
})
export class ProgramListComponent implements OnInit {
  private programService = inject(ProgramService);
  private dialog = inject(MatDialog);

  loading = signal(true);
  programs = signal<Program[]>([]);

  columns: TableColumnConfig<Program>[] = [
    { key: 'name', label: 'Name' },
    { key: 'code', label: 'Code', mono: true },
    { key: 'organizationName', label: 'Organization' },
    { key: 'startDate', label: 'Start date', formatter: (r: Program) => r.startDate ? new Date(r.startDate).toLocaleDateString() : '—' },
    { key: 'endDate', label: 'End date', formatter: (r: Program) => r.endDate ? new Date(r.endDate).toLocaleDateString() : '—' },
    { key: 'isActive', label: 'Status', formatter: (r: Program) => r.isActive ? 'Active' : 'Inactive' }
  ];

  private readonly fields: FormFieldConfig[] = [
    { key: 'name', label: 'Program name', type: 'text', required: true },
    { key: 'code', label: 'Code', type: 'text', required: true },
    { key: 'description', label: 'Description', type: 'textarea' },
    { key: 'startDate', label: 'Start date', type: 'date', required: true },
    { key: 'endDate', label: 'End date', type: 'date', required: true },
    { key: 'totalWeeks', label: 'Total weeks', type: 'number', required: true }
  ];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.programService.list({ pageSize: 100 }).subscribe({
      next: (result) => {
        this.programs.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: 'Add program', fields: this.fields, submitText: 'Create' }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.programService.create(value as Program).subscribe(() => {
        this.load();
      });
    });
  }

  openEdit(program: Program): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: 'Edit program', fields: this.fields, model: program, submitText: 'Save' }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.programService.update(program.id, value as Partial<Program>).subscribe(() => {
        this.load();
      });
    });
  }

  viewDetails(program: Program): void {
    // navigation handled by routerLink in template
  }

  toggleActive(program: Program): void {
    const action = program.isActive ? 'deactivate' : 'activate';
    const obs = program.isActive
      ? this.programService.deactivate(program.id)
      : this.programService.activate(program.id);
    obs.subscribe(() => {
      this.load();
    });
  }
}