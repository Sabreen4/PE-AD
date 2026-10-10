import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '@features/identity/services/user.service';
import { OrganizationService } from '@features/identity/services/organization.service';
import { User } from '@features/identity/models/user.model';

import { TableColumnConfig, FormFieldConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { IDENTITY_LABELS } from '@features/identity/constants/identity.constants';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent implements OnInit {
  private userService = inject(UserService);
  private organizationService = inject(OrganizationService);
  private dialog = inject(MatDialog);

  public readonly labels = IDENTITY_LABELS.users;

  loading = signal(true);
  users = signal<User[]>([]);
  organizationOptions = signal<{ value: string; label: string }[]>([]);

  columns: TableColumnConfig<User>[] = [
    { key: 'firstName', label: this.labels.columns.firstName },
    { key: 'lastName', label: this.labels.columns.lastName },
    { key: 'email', label: this.labels.columns.email },
    { key: 'roles', label: this.labels.columns.roles, formatter: (row: User) => (row.roles?.join(', ') ?? '') || this.labels.emptyRolesDisplay },
    { key: 'organizationName', label: this.labels.columns.organization },
    { key: 'isActive', label: this.labels.columns.status, formatter: (row: User) => (row.isActive === false ? this.labels.statusInactive : this.labels.statusActive) }
  ];

  ngOnInit(): void {
    this.organizationService.list({ pageSize: 200 }).subscribe((result) => {
      this.organizationOptions.set(result.items.map((o) => ({ value: o.id, label: o.name })));
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.userService.list({ pageSize: 100 }).subscribe({
      next: (result) => {
        this.users.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private fields(model?: User): FormFieldConfig[] {
    return [
      { key: 'email', label: this.labels.fields.email, type: 'text', required: true, disabled: !!model },
      { key: 'firstName', label: this.labels.fields.firstName, type: 'text', required: true },
      { key: 'lastName', label: this.labels.fields.lastName, type: 'text', required: true },
      { key: 'organizationId', label: this.labels.fields.organization, type: 'select', required: true, options: this.organizationOptions() },
      {
        key: 'roles',
        label: this.labels.fields.roles,
        type: 'text',
        hint: this.labels.fields.rolesHint,
      },
      ...(model ? [{ key: 'isActive', label: this.labels.fields.active, type: 'checkbox' as const }] : [])
    ];
  }

  openCreate(): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: this.labels.dialogAddTitle, fields: this.fields(), submitText: this.labels.dialogInviteText }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      const payload = { ...value, roles: this.splitRoles(value['roles']) };
      this.userService.create(payload).subscribe(() => {
        this.load();
      });
    });
  }

  openEdit(user: User): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: { title: this.labels.dialogEditTitle, fields: this.fields(user), model: { ...user, roles: '' }, submitText: this.labels.dialogSaveText }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      const payload = { ...value, roles: this.splitRoles(value['roles']) };
      this.userService.update(user.id, payload).subscribe(() => {
        this.load();
      });
    });
  }

  deactivate(user: User): void {
    this.userService.deactivate(user.id).subscribe(() => {
      this.load();
    });
  }

  private splitRoles(raw: string): string[] {
    if (typeof raw !== 'string' || !raw.trim()) return [];
    return raw.split(',').map((r) => r.trim()).filter(Boolean);
  }
}
