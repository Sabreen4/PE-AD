import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import {
  NotificationService,
  NotificationDto,
  getNotificationType,
  NotificationType,
} from '@core/services/notification.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ToastService } from '@shared/toast/toast.service';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';

function relativeTime(dateStr: string): string {
  const now = Date.now();
  const date = new Date(dateStr).getTime();
  const diffMs = now - date;
  const seconds = Math.floor(diffMs / 1000);
  if (seconds < 60) return 'Just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days === 1) return 'Yesterday';
  if (days < 7) return `${days}d ago`;
  return dateStr ? new Date(dateStr).toLocaleDateString() : '';
}

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    BackNavComponent,
  ],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss',
})
export class NotificationsComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthenticationStateService);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly service = this.notificationService;
  protected readonly relativeTime = relativeTime;

  ngOnInit(): void {
    this.service.loadNotifications();
  }

  protected notificationType(title: string, message: string): NotificationType {
    return getNotificationType(title, message);
  }

  protected typeIcon(nType: NotificationType): string {
    switch (nType) {
      case 'program_published': return 'campaign';
      case 'assignment_submitted': return 'assignment_turned_in';
      case 'assignment_evaluated': return 'rate_review';
      case 'weekly_score': return 'bar_chart';
      case 'feedback': return 'feedback';
      case 'attendance': return 'fact_check';
      default: return 'notifications';
    }
  }

  protected typeColor(nType: NotificationType): string {
    switch (nType) {
      case 'program_published': return '#3b82f6';
      case 'assignment_submitted': return '#f59e0b';
      case 'assignment_evaluated': return '#8b5cf6';
      case 'weekly_score': return '#10b981';
      case 'feedback': return '#ec4899';
      case 'attendance': return '#06b6d4';
      default: return '#6b7280';
    }
  }

  protected trackById(_index: number, item: NotificationDto): number {
    return item.id;
  }

  protected markRead(notification: NotificationDto): void {
    if (notification.isRead) return;
    this.service.markAsRead(notification.id);
  }

  protected markAllRead(): void {
    this.service.markAllAsRead();
    this.toast.success('Done', 'All notifications marked as read.');
  }

  protected delete(notification: NotificationDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Notification',
        message: `Are you sure you want to delete "${notification.title}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        danger: true,
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.service.deleteNotification(notification.id);
    });
  }

  protected onClick(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.service.markAsRead(notification.id);
    }
    const route = this.guessRoute(notification.title);
    if (route) {
      this.router.navigate(route);
    }
  }

  private guessRoute(title: string): string[] | null {
    const role = this.authState.role();
    let prefix = '';
    if (role === 'Evaluator') prefix = '/evaluator';
    else if (role === 'Employee') prefix = '/employee';
    else if (role === 'Admin') prefix = '/admin';
    if (!prefix) return null;

    const t = title.toLowerCase();
    if (t.includes('program')) return [prefix, 'programs'];
    if (t.includes('assignment')) return [prefix, 'programs'];
    if (t.includes('score')) return [prefix, 'weekly-scores'];
    if (t.includes('feedback')) return [prefix, 'feedback'];
    if (t.includes('attendance')) return [prefix, 'attendance'];
    return null;
  }
}
