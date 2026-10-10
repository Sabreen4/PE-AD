import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { UserService } from '@features/identity/services/user.service';
import { User } from '@features/identity/models/user.model';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

type Tab = 'employees' | 'evaluators';
type StatusFilter = 'all' | 'active' | 'inactive';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, BackNavComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly userService: UserService = inject(UserService);

  public readonly currentUser = this.authState.user;
  public readonly orgId = computed<number>(() => this.currentUser()?.organizationId ?? 0);

  public readonly activeTab = signal<Tab>('employees');
  public readonly searchQuery = signal<string>('');
  public readonly statusFilter = signal<StatusFilter>('all');
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = PAGE_SIZE;

  public readonly allUsers = signal<User[]>([]);
  public readonly isLoading = signal<boolean>(false);
  public readonly errorMessage = signal<string | null>(null);

  public ngOnInit(): void {
    this.loadUsers();
  }

  public switchTab(tab: Tab): void {
    this.activeTab.set(tab);
    this.searchQuery.set('');
    this.statusFilter.set('all');
    this.currentPage.set(1);
    this.errorMessage.set(null);
  }

  public onSearch(value: string): void {
    this.searchQuery.set(value);
    this.currentPage.set(1);
  }

  public onStatusFilter(value: string): void {
    this.statusFilter.set(value as StatusFilter);
    this.currentPage.set(1);
  }

  public goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
  }

  private readonly roleFiltered = computed<User[]>(() => {
    const targetRole = this.activeTab() === 'employees' ? 'employee' : 'evaluator';
    const currentOrgId = this.orgId();
    return this.allUsers().filter((u) => {
      if (currentOrgId && u.organizationId && Number(u.organizationId) !== currentOrgId) {
        return false;
      }
      const r = (u.role || '').toLowerCase();
      const roles = (u.roles || []).map((x) => x.toLowerCase());
      return r === targetRole || roles.includes(targetRole);
    });
  });

  public readonly filtered = computed<User[]>(() => {
    const items = this.roleFiltered();
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

  public readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.filtered().length / this.pageSize))
  );

  public readonly pagedItems = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.filtered().slice(start, start + this.pageSize);
  });

  public readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  });

  public readonly filteredCount = computed(() => this.filtered().length);

  private loadUsers(): void {
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

  public deactivateUser(id: string): void {
    this.userService.deactivate(id).subscribe({
      next: () => this.loadUsers(),
      error: () => {
        this.errorMessage.set('Failed to deactivate user.');
      },
    });
  }
}
