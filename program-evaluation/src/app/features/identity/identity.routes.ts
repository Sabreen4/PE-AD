import { Routes } from '@angular/router';

export const IDENTITY_ROUTES: Routes = [
  {
    path: 'organizations',
    loadComponent: () => import('./organizations/organizations-list.component').then((m) => m.OrganizationsListComponent),
    title: 'Organizations — Program Evaluation'
  },
  {
    path: 'users',
    loadComponent: () => import('./users/users-list.component').then((m) => m.UsersListComponent),
    title: 'Users — Program Evaluation'
  },
  {
    path: 'employees',
    loadComponent: () => import('./employees/employees-list.component').then((m) => m.EmployeesListComponent),
    title: 'Employees — Program Evaluation'
  }
];
