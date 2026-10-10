import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService, NotificationDto, getNotificationType, NotificationType } from '@core/services/notification.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';

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
  selector: 'app-notification-drawer',
  standalone: true,
  imports: [CommonModule, MatIconModule, EmptyStateComponent, ConfirmDialogComponent],
  templateUrl: './notification-drawer.component.html',
  styleUrl: './notification-drawer.component.scss',
})
export class NotificationDrawerComponent {
  protected readonly service = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthenticationStateService);
  private readonly dialog = inject(MatDialog);

  protected readonly rolePrefix = computed(() => {
    const role = this.authState.role();
    if (role === 'Evaluator') return '/evaluator';
    if (role === 'Employee') return '/employee';
    if (role === 'Admin') return '/admin';
    return '';
  });

  protected readonly relativeTime = relativeTime;

  protected notificationType(type: string, title: string, message: string): NotificationType {
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

  protected onClick(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.service.markAsRead(notification.id);
    }
    this.service.closeDrawer();
    const route = this.guessRoute(notification.title, notification.message);
    if (route) {
      this.router.navigate(route);
    }
  }

  protected delete(notification: NotificationDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '380px',
      data: {
        title: 'Delete Notification',
        message: 'Are you sure you want to delete this notification?',
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

  private guessRoute(title: string, _message: string): string[] | null {
    const prefix = this.rolePrefix();
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
