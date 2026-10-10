import { Routes } from '@angular/router';

export const EVALUATION_ROUTES: Routes = [
  {
    path: 'attendance',
    loadComponent: () => import('./attendance/attendance.component').then((m) => m.AttendanceComponent),
    title: 'Attendance � Program Evaluation'
  },
  {
    path: 'weekly-evaluations',
    loadComponent: () => import('./weekly-evaluations/weekly-evaluations.component').then((m) => m.WeeklyEvaluationsComponent),
    title: 'Weekly evaluations � Program Evaluation'
  },
  {
    path: 'assessment-evaluations',
    loadComponent: () => import('./assessment-evaluations/assessment-evaluations.component').then((m) => m.AssessmentEvaluationsComponent),
    title: 'Assessment evaluations � Program Evaluation'
  },
  {
    path: 'qualitative-feedback',
    loadComponent: () => import('./qualitative-feedback/qualitative-feedback.component').then((m) => m.QualitativeFeedbackComponent),
    title: 'Qualitative feedback � Program Evaluation'
  },
  {
    path: 'leaderboard',
    loadComponent: () => import('./leaderboard/leaderboard.component').then((m) => m.LeaderboardComponent),
    title: 'Leaderboard � Program Evaluation'
  }
];
