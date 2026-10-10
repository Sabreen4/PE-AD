import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { OrganizationService } from '@features/identity/services/organization.service';
import { Organization } from '@features/identity/models/organization.model';
import { TableColumnConfig, FormFieldConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { IDENTITY_LABELS } from '@features/identity/constants/identity.constants';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-organizations-list',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './organizations-list.component.html'
})
export class OrganizationsListComponent implements OnInit {
  private organizationService = inject(OrganizationService);
  private dialog = inject(MatDialog);

  public readonly labels = IDENTITY_LABELS.organizations;

  loading = signal(true);
  organizations = signal<Organization[]>([]);

  columns: TableColumnConfig<Organization>[] = [
    { key: 'name', label: this.labels.columns.name },
    { key: 'code', label: this.labels.columns.code },
    { key: 'isActive', label: this.labels.columns.status, formatter: (row: Organization) => (row.isActive === false ? this.labels.statusInactive : this.labels.statusActive) },
    { key: 'createdAt', label: this.labels.columns.created, formatter: (row: Organization) => new Date(row.createdAt).toLocaleDateString() }
  ];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.organizationService.list({ pageSize: 100 }).subscribe({
      next: (result) => {
        this.organizations.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private fields(model?: Organization): FormFieldConfig[] {
    return [
      { key: 'name', label: this.labels.fields.name, type: 'text', required: true },
      { key: 'code', label: this.labels.fields.code, type: 'text', required: true },
      { key: 'description', label: this.labels.fields.description, type: 'textarea' },
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
      this.organizationService.create(value).subscribe(() => {
        this.load();
      });
    });
  }

  openEdit(organization: Organization): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: this.labels.dialogEditTitle, fields: this.fields(organization), model: organization, submitText: this.labels.dialogSaveText }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.organizationService.update(organization.id, value).subscribe(() => {
        this.load();
      });
    });
  }

  deactivate(organization: Organization): void {
    this.organizationService.deactivate(organization.id).subscribe(() => {
      this.load();
    });
  }
}
