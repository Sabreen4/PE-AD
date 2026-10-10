import { Routes } from '@angular/router';

export const ASSIGNMENT_ROUTES: Routes = [
  {
    path: 'assignment-templates',
    loadComponent: () => import('./assignment-template-list/assignment-template-list.component').then((m) => m.AssignmentTemplateListComponent),
    title: 'Assignment templates � Program Evaluation'
  },
  {
    path: 'employee-assignments',
    loadComponent: () => import('./employee-assignment-list/employee-assignment-list.component').then((m) => m.EmployeeAssignmentListComponent),
    title: 'Employee assignments � Program Evaluation'
  },
  {
    path: 'submissions',
    loadComponent: () => import('./submission-list/submission-list.component').then((m) => m.SubmissionListComponent),
    title: 'Submissions � Program Evaluation'
  }
];
