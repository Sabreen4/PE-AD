import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, Subject, from } from 'rxjs';
import { NotificationService as NotificationApiService } from '@features/notification/services/NotificationService';
import { SignalRService, SignalRNotification } from './signalr.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { ToastService } from '@shared/toast/toast.service';

export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  channel: string;
  isRead: boolean;
  createdAt: string;
}

export interface PagedNotificationResponse {
  items: NotificationDto[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export type NotificationType = 'program_published' | 'assignment_submitted' | 'assignment_evaluated' | 'weekly_score' | 'feedback' | 'attendance' | 'info';

export function getNotificationType(title: string, message: string): NotificationType {
  const t = title.toLowerCase();
  const m = message.toLowerCase();
  if (t.includes('program') && m.includes('publish')) return 'program_published';
  if (t.includes('assignment') && m.includes('submitted')) return 'assignment_submitted';
  if (t.includes('assignment') && (m.includes('evaluat') || m.includes('reviewed'))) return 'assignment_evaluated';
  if (t.includes('score') || t.includes('weekly')) return 'weekly_score';
  if (t.includes('feedback')) return 'feedback';
  if (t.includes('attendance')) return 'attendance';
  return 'info';
}

const PAGE_SIZE = 50;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly api = inject(NotificationApiService);
  private readonly signalr = inject(SignalRService);
  private readonly authState = inject(AuthenticationStateService);
  private readonly toast = inject(ToastService);

  constructor() {
    if (this.authState.isAuthenticated()) {
      from(this.signalr.start()).subscribe({
        next: () => this.init(),
        error: () => console.warn('[Notification] SignalR unavailable — notifications disabled'),
      });
    }
  }

  private readonly notificationsSignal = signal<NotificationDto[]>([]);
  private readonly totalCountSignal = signal(0);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);
  private readonly drawerOpenSignal = signal(false);

  public readonly notifications = this.notificationsSignal.asReadonly();
  public readonly totalCount = this.totalCountSignal.asReadonly();
  public readonly loading = this.loadingSignal.asReadonly();
  public readonly error = this.errorSignal.asReadonly();
  public readonly drawerOpen = this.drawerOpenSignal.asReadonly();

  public readonly unreadCount = computed(() =>
    this.notificationsSignal().filter((n) => !n.isRead).length
  );

  public readonly latestReceived = signal<NotificationDto | null>(null);

  private isInitialized = false;

  public init(): void {
    if (this.isInitialized) return;
    this.isInitialized = true;

    this.loadNotifications();

    this.signalr.notificationReceived.subscribe((n: SignalRNotification) => {
      this.toast.info(n.title, n.message);
      this.loadNotifications();
    });
  }

  public loadNotifications(): void {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);
    this.api.getNotifications(undefined, 1, PAGE_SIZE).subscribe({
      next: (res) => {
        const data = res as {} as PagedNotificationResponse;
        this.notificationsSignal.set(data?.items ?? []);
        this.totalCountSignal.set(data?.totalRecords ?? 0);
        this.loadingSignal.set(false);
      },
      error: (err: Error) => {
        this.errorSignal.set('Failed to load notifications.');
        this.loadingSignal.set(false);
      },
    });
  }

  public markAsRead(id: number): void {
    this.api.putNotificationsMarkAsRead(id).subscribe({
      next: () => {
        this.notificationsSignal.update((list) =>
          list.map((n) => (n.id === id ? { ...n, isRead: true } : n))
        );
      },
    });
  }

  public markAllAsRead(): void {
    this.api.putNotificationsMarkAllAsRead().subscribe({
      next: () => {
        this.notificationsSignal.update((list) =>
          list.map((n) => ({ ...n, isRead: true }))
        );
      },
    });
  }

  public deleteNotification(id: number): void {
    this.api.deleteNotifications(id).subscribe({
      next: () => {
        this.notificationsSignal.update((list) => list.filter((n) => n.id !== id));
        this.toast.success('Deleted', 'Notification deleted successfully.');
      },
      error: () => {
        this.toast.error('Error', 'Failed to delete notification.');
      },
    });
  }

  public openDrawer(): void {
    this.drawerOpenSignal.set(true);
    this.loadNotifications();
  }

  public closeDrawer(): void {
    this.drawerOpenSignal.set(false);
  }

  public toggleDrawer(): void {
    if (this.drawerOpenSignal()) {
      this.closeDrawer();
    } else {
      this.openDrawer();
    }
  }
}
