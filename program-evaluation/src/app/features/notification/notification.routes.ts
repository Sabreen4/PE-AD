import { Routes } from '@angular/router';

export const NOTIFICATION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./notifications/notifications.component').then((m) => m.NotificationsComponent),
    title: 'Notifications - Program Evaluation'
  }
];
