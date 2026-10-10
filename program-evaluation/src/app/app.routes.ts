import { Routes } from '@angular/router';
import { authenticationGuard } from '@core/guards/authentication.guard';
import { MainLayoutComponent } from '@layouts/main-layout/main-layout.component';
import { AuthenticationLayoutComponent } from '@layouts/authentication-layout/authentication-layout.component';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthenticationLayoutComponent,
    loadChildren: () => import('@features/identity/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'register',
    loadChildren: () =>
      import('@features/identity/registration/registration.routes').then((m) => m.REGISTRATION_ROUTES),
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authenticationGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadChildren: () => import('@features/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
      },
      {
        path: 'identity',
        loadChildren: () => import('@features/identity/identity.routes').then((m) => m.IDENTITY_ROUTES),
      },
      {
        path: 'notifications',
        loadChildren: () =>
          import('@features/notification/notification.routes').then((m) => m.NOTIFICATION_ROUTES),
      },
      {
        path: 'program',
        loadChildren: () => import('@features/program/program.routes').then((m) => m.PROGRAM_ROUTES),
      },
      {
        path: 'assignments',
        loadChildren: () => import('@features/assignment/assignment.routes').then((m) => m.ASSIGNMENT_ROUTES),
      },
      {
        path: 'evaluation',
        loadChildren: () => import('@features/evaluation/evaluation.routes').then((m) => m.EVALUATION_ROUTES),
      },
    ],
  },
  {
    path: 'admin',
    loadChildren: () => import('@features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
  },
  {
    path: 'employee',
    loadChildren: () => import('@features/employee/employee.routes').then((m) => m.EMPLOYEE_ROUTES),
  },
  {
    path: 'evaluator',
    loadChildren: () => import('@features/evaluator/evaluator.routes').then((m) => m.EVALUATOR_ROUTES),
  },
  {
    path: 'access-denied',
    title: 'Access Denied \u2014 PE&DA',
    loadComponent: () =>
      import('./shared/components/access-denied.component').then((m) => m.AccessDeniedComponent),
  },
  {
    path: 'not-found',
    title: 'Not Found \u2014 PE&DA',
    loadComponent: () =>
      import('./shared/components/not-found.component').then((m) => m.NotFoundComponent),
  },
  { path: '**', redirectTo: 'not-found' },
];
