import { Routes } from '@angular/router';
import { authenticationGuard } from '@core/guards/authentication.guard';
import { roleGuard } from '@core/guards/role.guard';
import { DashboardLayoutComponent } from '@shared/dashboard-layout/dashboard-layout.component';

export const EMPLOYEE_ROUTES: Routes = [
  {
    path: '',
    component: DashboardLayoutComponent,
    canActivate: [authenticationGuard, roleGuard('Employee')],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then((m) => m.EmployeeDashboardComponent),
        title: 'Dashboard \u2014 Employee',
      },
      {
        path: 'programs',
        loadComponent: () =>
          import('./pages/programs/programs.component').then((m) => m.EmployeeProgramsComponent),
        title: 'Programs \u2014 Employee',
      },
      {
        path: 'programs/:id',
        loadComponent: () =>
          import('./pages/program-detail/program-detail.component').then((m) => m.EmployeeProgramDetailComponent),
        title: 'Program Details \u2014 Employee',
      },
      {
        path: 'programs/:id/weeks/:weekId',
        loadComponent: () =>
          import('./pages/week-detail/week-detail.component').then((m) => m.EmployeeWeekDetailComponent),
        title: 'Week Details \u2014 Employee',
      },
      {
        path: 'programs/:id/weeks/:weekId/assignments/:assignmentId',
        loadComponent: () =>
          import('./pages/assignment-view/assignment-view.component').then((m) => m.AssignmentViewComponent),
        title: 'Assignment \u2014 Employee',
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./pages/assignments/assignments.component').then((m) => m.EmployeeAssignmentsComponent),
        title: 'Assignments \u2014 Employee',
      },
      {
        path: 'progress-timeline',
        loadComponent: () =>
          import('./pages/progress-timeline/progress-timeline.component').then((m) => m.ProgressTimelineComponent),
        title: 'Weekly Timeline \u2014 Employee',
      },
      {
        path: 'leaderboard',
        loadComponent: () =>
          import('./pages/leaderboard/leaderboard.component').then((m) => m.EmployeeLeaderboardComponent),
        title: 'Leaderboard \u2014 Employee',
      },
      {
        path: 'weekly-score',
        loadComponent: () =>
          import('./pages/weekly-score/weekly-score.component').then((m) => m.EmployeeWeeklyScoreComponent),
        title: 'Weekly Score \u2014 Employee',
      },
      {
        path: 'attendance',
        loadComponent: () =>
          import('./pages/attendance/attendance.component').then((m) => m.EmployeeAttendanceComponent),
        title: 'Attendance \u2014 Employee',
      },
      {
        path: 'feedback',
        loadComponent: () =>
          import('./pages/feedback/feedback.component').then((m) => m.EmployeeFeedbackComponent),
        title: 'Feedback \u2014 Employee',
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./pages/profile/profile.component').then((m) => m.EmployeeProfileComponent),
        title: 'Profile \u2014 Employee',
      },
    ],
  },
];
