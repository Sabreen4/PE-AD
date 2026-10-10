import { Routes } from '@angular/router';
import { authenticationGuard } from '@core/guards/authentication.guard';
import { roleGuard } from '@core/guards/role.guard';
import { DashboardLayoutComponent } from '@shared/dashboard-layout/dashboard-layout.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: DashboardLayoutComponent,
    canActivate: [authenticationGuard, roleGuard('Admin')],
    children: [
      { path: '', redirectTo: 'employees', pathMatch: 'full' },
      {
        path: 'employees',
        loadComponent: () =>
          import('./pages/employee-management/employee-management.component')
            .then((m) => m.EmployeeManagementComponent),
        title: 'Employees \u2014 Admin',
      },
      {
        path: 'evaluators',
        loadComponent: () =>
          import('./pages/evaluator-management/evaluator-management.component')
            .then((m) => m.EvaluatorManagementComponent),
        title: 'Evaluators \u2014 Admin',
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./pages/profile/profile.component')
            .then((m) => m.ProfileComponent),
        title: 'Profile \u2014 Admin',
      },
    ],
  },
];
