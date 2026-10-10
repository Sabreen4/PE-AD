import { Routes } from '@angular/router';
import { guestGuard } from '@core/guards/guest.guard';
import { authenticationGuard } from '@core/guards/authentication.guard';
import { roleGuard } from '@core/guards/role.guard';

export const REGISTRATION_ROUTES: Routes = [
  {
    path: 'organization',
    loadComponent: () =>
      import(
        './organization-registration/organization-registration.component'
      ).then((m) => m.OrganizationRegistrationComponent),
    canActivate: [guestGuard],
    title: 'Register Organization \u2014 PE&DA',
  },
  {
    path: 'employees/register',
    loadComponent: () =>
      import(
        './employee-registration/employee-registration.component'
      ).then((m) => m.EmployeeRegistrationComponent),
    canActivate: [authenticationGuard, roleGuard('Admin')],
    title: 'Register Employee \u2014 PE&DA',
  },
  {
    path: 'evaluators/register',
    loadComponent: () =>
      import(
        './evaluator-registration/evaluator-registration.component'
      ).then((m) => m.EvaluatorRegistrationComponent),
    canActivate: [authenticationGuard, roleGuard('Admin')],
    title: 'Register Evaluator \u2014 PE&DA',
  },
];
