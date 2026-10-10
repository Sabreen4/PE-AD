import { Routes } from '@angular/router';

export const PROGRAM_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./program-list/program-list.component').then((m) => m.ProgramListComponent),
    title: 'Programs — Program Evaluation'
  },
  {
    path: ':id',
    loadComponent: () => import('./program-details/program-details.component').then((m) => m.ProgramDetailsComponent),
    title: 'Program details — Program Evaluation',
    children: [
      {
        path: 'weeks',
        loadComponent: () => import('./program-week/program-week.component').then((m) => m.ProgramWeekComponent)
      },
      {
        path: 'enrollments',
        loadComponent: () => import('./enrollment/enrollment.component').then((m) => m.EnrollmentComponent)
      },
      {
        path: 'activities',
        loadComponent: () => import('./program-activity/program-activity.component').then((m) => m.ProgramActivityComponent)
      }
    ]
  }
];