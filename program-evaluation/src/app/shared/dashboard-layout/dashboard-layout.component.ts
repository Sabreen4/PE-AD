import { Component, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '@shared/navbar/navbar.component';
import { SidebarComponent, NavItem } from '@shared/sidebar/sidebar.component';
import { ToastComponent } from '@shared/toast/toast.component';
import { NotificationDrawerComponent } from '@shared/components/notification-drawer/notification-drawer.component';
import { SettingsPanelComponent } from '@shared/components/settings-panel/settings-panel.component';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { UserRole } from '@core/models/identity/user-role.enum';

const ADMIN_NAV_ITEMS: NavItem[] = [
  { label: 'Employees', route: '/admin/employees', icon: 'group' },
  { label: 'Evaluators', route: '/admin/evaluators', icon: 'manage_accounts' },
];

const EVALUATOR_NAV_ITEMS: NavItem[] = [
  { label: 'Programs', route: '/evaluator/programs', icon: 'school' },
  { label: 'Attendance', route: '/evaluator/attendance', icon: 'fact_check' },
  { label: 'Weekly Scores', route: '/evaluator/weekly-scores', icon: 'bar_chart' },
  { label: 'Assignment Submissions', route: '/evaluator/assignment-submissions', icon: 'assignment' },
  { label: 'Feedback', route: '/evaluator/feedback', icon: 'feedback' },
  { label: 'Leaderboard', route: '/evaluator/leaderboard', icon: 'leaderboard' },
];

const EMPLOYEE_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', route: '/employee/dashboard', icon: 'dashboard' },
  { label: 'Programs', route: '/employee/programs', icon: 'school' },
  { label: 'Weekly Timeline', route: '/employee/progress-timeline', icon: 'timeline' },
  { label: 'Assignments', route: '/employee/assignments', icon: 'assignment' },
  { label: 'Attendance', route: '/employee/attendance', icon: 'fact_check' },
  { label: 'Feedback', route: '/employee/feedback', icon: 'feedback' },
  { label: 'Weekly Score', route: '/employee/weekly-score', icon: 'bar_chart' },
  { label: 'Leaderboard', route: '/employee/leaderboard', icon: 'leaderboard' },
];

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavbarComponent, SidebarComponent, ToastComponent, NotificationDrawerComponent, SettingsPanelComponent],
  templateUrl: './dashboard-layout.component.html',
  styleUrl: './dashboard-layout.component.scss'
})
export class DashboardLayoutComponent {
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);

  public readonly collapsed = signal<boolean>(
    typeof localStorage !== 'undefined' && localStorage.getItem('sidebarCollapsed') === 'true'
  );

  public readonly mobileNavOpen = signal<boolean>(false);
  public readonly settingsOpen = signal<boolean>(false);

  public readonly brand = computed<string>(() => {
    const role = this.authState.role();
    if (role === 'Evaluator') return 'Evaluator';
    if (role === 'Employee') return 'Employee';
    return 'Admin';
  });

  public readonly navItems = computed<NavItem[]>(() => {
    const role = this.authState.role();
    if (role === UserRole.Evaluator) return EVALUATOR_NAV_ITEMS;
    if (role === UserRole.Employee) return EMPLOYEE_NAV_ITEMS;
    return ADMIN_NAV_ITEMS;
  });

  public toggleCollapsed(): void {
    const next = !this.collapsed();
    this.collapsed.set(next);
    localStorage.setItem('sidebarCollapsed', String(next));
  }

  public toggleMobileNav(): void {
    this.mobileNavOpen.update((v) => !v);
  }

  public closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }

  public toggleSettings(): void {
    this.settingsOpen.update((v) => !v);
  }

  public closeSettings(): void {
    this.settingsOpen.set(false);
  }
}
