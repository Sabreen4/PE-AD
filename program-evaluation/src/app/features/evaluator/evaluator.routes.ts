import { Routes } from '@angular/router';
import { authenticationGuard } from '@core/guards/authentication.guard';
import { roleGuard } from '@core/guards/role.guard';
import { DashboardLayoutComponent } from '@shared/dashboard-layout/dashboard-layout.component';

export const EVALUATOR_ROUTES: Routes = [
  {
    path: '',
    component: DashboardLayoutComponent,
    canActivate: [authenticationGuard, roleGuard('Evaluator')],
    children: [
      { path: '', redirectTo: 'programs', pathMatch: 'full' },
      {
        path: 'programs',
        loadComponent: () =>
          import('./pages/programs/programs.component').then((m) => m.ProgramsComponent),
        title: 'Programs \u2014 Evaluator',
      },
      {
        path: 'programs/create',
        loadComponent: () =>
          import('./pages/programs/create/create-program.component').then((m) => m.CreateProgramComponent),
        title: 'Create Program \u2014 Evaluator',
      },
      {
        path: 'programs/:id',
        loadComponent: () =>
          import('./pages/program-details/program-details.component').then((m) => m.ProgramDetailsComponent),
        title: 'Program Details \u2014 Evaluator',
      },
      {
        path: 'programs/:id/weeks',
        loadComponent: () =>
          import('./pages/program-weeks/program-weeks.component').then((m) => m.ProgramWeeksComponent),
        title: 'Program Weeks \u2014 Evaluator',
      },
      {
        path: 'programs/:id/weeks/create',
        loadComponent: () =>
          import('./pages/create-week/create-week.component').then((m) => m.CreateWeekComponent),
        title: 'Create Week \u2014 Evaluator',
      },
      {
        path: 'programs/:id/weeks/:weekId',
        loadComponent: () =>
          import('./pages/week-detail/week-detail.component').then((m) => m.WeekDetailComponent),
        title: 'Week Details \u2014 Evaluator',
      },
      {
        path: 'programs/:id/weeks/:weekId/assignments/create',
        loadComponent: () =>
          import('./pages/create-assignment/create-assignment.component').then((m) => m.CreateAssignmentComponent),
        title: 'Create Assignment \u2014 Evaluator',
      },
      {
        path: 'programs/:id/weeks/:weekId/assignments/:assignmentId',
        loadComponent: () =>
          import('./pages/assignment-detail/assignment-detail.component').then((m) => m.AssignmentDetailComponent),
        title: 'Assignment Details \u2014 Evaluator',
      },
      {
        path: 'programs/:id/weeks/:weekId/assignments/:assignmentId/edit',
        loadComponent: () =>
          import('./pages/edit-assignment/edit-assignment.component').then((m) => m.EditAssignmentComponent),
        title: 'Edit Assignment \u2014 Evaluator',
      },
      {
        path: 'attendance',
        loadComponent: () =>
          import('./pages/attendance/attendance.component').then((m) => m.AttendanceComponent),
        title: 'Attendance \u2014 Evaluator',
      },
      {
        path: 'weekly-scores',
        loadComponent: () =>
          import('./pages/weekly-scores/weekly-scores.component').then((m) => m.WeeklyScoresComponent),
        title: 'Weekly Scores \u2014 Evaluator',
      },
      {
        path: 'assignment-submissions',
        loadComponent: () =>
          import('./pages/assignment-submissions/assignment-submissions.component').then((m) => m.AssignmentSubmissionsComponent),
        title: 'Assignment Submissions \u2014 Evaluator',
      },
      {
        path: 'feedback',
        loadComponent: () =>
          import('./pages/feedback/feedback.component').then((m) => m.FeedbackComponent),
        title: 'Feedback \u2014 Evaluator',
      },
      {
        path: 'leaderboard',
        loadComponent: () =>
          import('./pages/leaderboard/leaderboard.component').then((m) => m.EvaluatorLeaderboardComponent),
        title: 'Leaderboard \u2014 Evaluator',
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./pages/profile/profile.component').then((m) => m.ProfileComponent),
        title: 'Profile \u2014 Evaluator',
      },
    ],
  },
];
