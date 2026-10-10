import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { EmployeeService } from '@features/identity/services/employee.service';
import { OrganizationService } from '@features/identity/services/organization.service';
import { Employee } from '@features/identity/models/employee.model';
import { TableColumnConfig, FormFieldConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { IDENTITY_LABELS } from '@features/identity/constants/identity.constants';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-employees-list',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './employees-list.component.html'
})
export class EmployeesListComponent implements OnInit {
  private employeeService = inject(EmployeeService);
  private organizationService = inject(OrganizationService);
  private dialog = inject(MatDialog);

  public readonly labels = IDENTITY_LABELS.employees;

  loading = signal(true);
  employees = signal<Employee[]>([]);
  organizationOptions = signal<{ value: string; label: string }[]>([]);
  selectedOrganizationId = signal<string | undefined>(undefined);

  columns: TableColumnConfig<Employee>[] = [
    { key: 'employeeCode', label: this.labels.columns.code },
    { key: 'firstName', label: this.labels.columns.firstName },
    { key: 'lastName', label: this.labels.columns.lastName },
    { key: 'email', label: this.labels.columns.email },
    { key: 'organizationName', label: this.labels.columns.organization },
    { key: 'isActive', label: this.labels.columns.status, formatter: (row: Employee) => (row.isActive === false ? this.labels.statusInactive : this.labels.statusActive) }
  ];

  ngOnInit(): void {
    this.organizationService.list({ pageSize: 200 }).subscribe((result) => {
      this.organizationOptions.set(result.items.map((o) => ({ value: o.id, label: o.name })));
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const params: Record<string, string | number | boolean | null> = { pageSize: 100 };
    const obs = this.selectedOrganizationId()
      ? this.employeeService.getByOrganization(this.selectedOrganizationId()!, params)
      : this.employeeService.list(params);
    obs.subscribe({
      next: (result) => {
        this.employees.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  filterByOrganization(organizationId: string): void {
    this.selectedOrganizationId.set(organizationId || undefined);
    this.load();
  }

  private fields(model?: Employee): FormFieldConfig[] {
    return [
      { key: 'employeeCode', label: this.labels.fields.employeeCode, type: 'text', required: true },
      { key: 'firstName', label: this.labels.fields.firstName, type: 'text', required: true },
      { key: 'lastName', label: this.labels.fields.lastName, type: 'text', required: true },
      { key: 'email', label: this.labels.fields.email, type: 'text', required: true },
      { key: 'phone', label: this.labels.fields.phone, type: 'text' },
      { key: 'organizationId', label: this.labels.fields.organization, type: 'select', required: true, options: this.organizationOptions() },
      ...(model ? [{ key: 'isActive', label: this.labels.fields.active, type: 'checkbox' as const }] : [])
    ];
  }

  openCreate(): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: this.labels.dialogAddTitle, fields: this.fields(), submitText: this.labels.dialogCreateText }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.employeeService.create(value).subscribe(() => {
        this.load();
      });
    });
  }

  openEdit(employee: Employee): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: this.labels.dialogEditTitle, fields: this.fields(employee), model: employee, submitText: this.labels.dialogSaveText }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.employeeService.update(employee.id, value).subscribe(() => {
        this.load();
      });
    });
  }

  deactivate(employee: Employee): void {
    this.employeeService.deactivate(employee.id).subscribe(() => {
      this.load();
    });
  }
}
