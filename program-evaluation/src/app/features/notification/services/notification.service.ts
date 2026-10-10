import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { NotificationService as MonolithNotificationService } from './NotificationService';
import { NotificationDto } from '@core/services/notification.service';


@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly svc = inject(MonolithNotificationService);

  list(params?: { pageSize?: number }): Observable<{ items: NotificationDto[]; totalRecords: number }> {
    return this.svc.getNotifications(undefined, 1, params?.pageSize ?? 10).pipe(
      map((res: Record<string, string | number | boolean | null | Array<Record<string, string | number | boolean | null>>>) => {
        const data = res as { items?: NotificationDto[]; totalRecords?: number };
        return { items: data?.items ?? [], totalRecords: data?.totalRecords ?? 0 };
      })
    );
  }

  markRead(id: number): Observable<void> {
    return this.svc.putNotificationsMarkAsRead(id).pipe(map(() => void 0));
  }

  markAllRead(): Observable<void> {
    return this.svc.putNotificationsMarkAllAsRead().pipe(map(() => void 0));
  }

  deleteNotification(id: number): Observable<void> {
    return this.svc.deleteNotifications(id).pipe(map(() => void 0));
  }
}
