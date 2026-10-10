import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';

import { UserService } from '@features/identity/services/user.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { User } from '@features/identity/models/user.model';
import { ActionMenuComponent, ActionMenuItem } from '@shared/components/action-menu/action-menu.component';
import { UserDetailDialogComponent } from '@features/admin/components/user-detail-dialog/user-detail-dialog.component';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { EntityFormDialogComponent, EntityFormDialogData } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { FormFieldConfig } from '@shared/common/entity-types';
import { ToastService } from '@shared/toast/toast.service';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

const ACTION_ITEMS: ActionMenuItem[] = [
  { key: 'edit', label: 'Edit', icon: 'edit' },
  { key: 'delete', label: 'Delete', icon: 'delete', danger: true },
];

const EDIT_FIELDS: FormFieldConfig[] = [
  { key: 'firstName', label: 'First Name', type: 'text', required: true },
  { key: 'lastName', label: 'Last Name', type: 'text', required: true },
  { key: 'email', label: 'Email', type: 'text', required: true },
  { key: 'phone', label: 'Phone', type: 'text' },
];

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ActionMenuComponent, PaginationComponent, BackNavComponent],
  templateUrl: './employee-management.component.html',
  styleUrl: './employee-management.component.scss'
})
export class EmployeeManagementComponent implements OnInit {
  private readonly userService: UserService = inject(UserService);
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly dialog: MatDialog = inject(MatDialog);
  private readonly toast: ToastService = inject(ToastService);

  public readonly allUsers = signal<User[]>([]);
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(10);
  public readonly searchQuery = signal<string>('');
  public readonly statusFilter = signal<string>('all');
  public readonly isLoading = signal<boolean>(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly actionItems = ACTION_ITEMS;

  private readonly roleEmployees = computed<User[]>(() => {
    const orgId = this.authState.user()?.organizationId;
    return this.allUsers().filter((u) => {
      if (orgId && u.organizationId && Number(u.organizationId) !== Number(orgId)) {
        return false;
      }
      const r = (u.role || '').toLowerCase();
      const roles = (u.roles || []).map((x) => x.toLowerCase());
      return r === 'employee' || roles.includes('employee');
    });
  });

  public readonly filteredEmployees = computed<User[]>(() => {
    const items = this.roleEmployees();
    const query = this.searchQuery().toLowerCase().trim();
    const filter = this.statusFilter();

    let result = items;
    if (query) {
      result = result.filter((e) => {
        const fn = (e.firstName || '').toLowerCase();
        const ln = (e.lastName || '').toLowerCase();
        const full = `${fn} ${ln}`.trim();
        const email = (e.email || '').toLowerCase();
        return fn.includes(query) || ln.includes(query) || full.includes(query) || email.includes(query);
      });
    }
    if (filter === 'active') {
      result = result.filter((e) => e.isActive !== false);
    } else if (filter === 'inactive') {
      result = result.filter((e) => e.isActive === false);
    }
    return result;
  });

  public readonly totalRecords = computed<number>(() => this.filteredEmployees().length);

  public readonly employees = computed<User[]>(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredEmployees().slice(start, start + this.pageSize());
  });

  public ngOnInit(): void {
    this.load();
  }

  public onSearchInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchQuery.set(val);
    this.currentPage.set(1);
  }

  public clearSearch(input: HTMLInputElement): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    input.value = '';
  }

  public onStatusFilter(value: string): void {
    this.statusFilter.set(value);
    this.currentPage.set(1);
  }

  public goToPage(page: number): void {
    this.currentPage.set(page);
  }

  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  public viewDetails(user: User): void {
    this.dialog.open(UserDetailDialogComponent, {
      width: '560px',
      data: user,
    });
  }

  public handleAction(action: string, user: User): void {
    if (action === 'edit') this.editUser(user);
    if (action === 'delete') this.deleteUser(user);
  }

  private editUser(user: User): void {
    const data: EntityFormDialogData = {
      title: `Edit ${user.firstName} ${user.lastName}`,
      fields: EDIT_FIELDS,
      model: {
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        phone: user.phone ?? '',
      },
      submitText: 'Save',
    };
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data,
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      this.userService.update(user.id, value as Record<string, string | number | boolean | null>).subscribe({
        next: () => {
          this.toast.success('Updated', `${user.firstName} ${user.lastName} updated successfully.`);
          this.load();
        },
        error: () => {
          this.toast.error('Error', `Failed to update ${user.firstName} ${user.lastName}.`);
        },
      });
    });
  }

  private deleteUser(user: User): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete User',
        message: `Are you sure you want to delete ${user.firstName} ${user.lastName}? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        danger: true,
      },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.userService.deactivate(user.id).subscribe({
        next: () => {
          this.toast.success('Deleted', `${user.firstName} ${user.lastName} has been deleted.`);
          this.load();
        },
        error: () => {
          this.toast.error('Error', `Failed to delete ${user.firstName} ${user.lastName}.`);
        },
      });
    });
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.userService.list().subscribe({
      next: (res) => {
        this.allUsers.set(res.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load users.');
        this.isLoading.set(false);
      },
    });
  }
}
