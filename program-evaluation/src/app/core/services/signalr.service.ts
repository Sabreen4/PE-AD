import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { TokenStorageService } from '@core/authentication/token-storage.service';
import { apiConfig } from '@core/config/api.config';

export interface SignalRNotification {
  title: string;
  message: string;
  timestamp: string;
}

export type SignalRConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly authState = inject(AuthenticationStateService);
  private readonly tokenStorage = inject(TokenStorageService);

  private connection: signalR.HubConnection | null = null;
  private readonly notificationSubject = new Subject<SignalRNotification>();

  public readonly connectionState = signal<SignalRConnectionState>('disconnected');
  public readonly notificationReceived: Observable<SignalRNotification> = this.notificationSubject.asObservable();

  public async start(): Promise<void> {
    if (this.connection && this.connection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    const token = this.tokenStorage.getAccessToken();
    if (!token) {
      return;
    }

    const hubUrl = `${apiConfig.gatewayUrl}/notificationHub`;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.tokenStorage.getAccessToken() ?? '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.connection.onreconnecting(() => {
      this.connectionState.set('reconnecting');
    });

    this.connection.onreconnected(async () => {
      this.connectionState.set('connected');
      await this.joinUserGroup();
    });

    this.connection.onclose(() => {
      this.connectionState.set('disconnected');
    });

    this.connection.on('Notification', (title: string, message: string, timestamp: string) => {
      this.notificationSubject.next({ title, message, timestamp });
    });

    try {
      this.connectionState.set('connecting');
      await this.connection.start();
      this.connectionState.set('connected');
      await this.joinUserGroup();
    } catch (err) {
      console.warn('[SignalR] Connection failed — real-time notifications unavailable:', err);
      this.connectionState.set('disconnected');
      this.connection = null;
    }
  }

  public async stop(): Promise<void> {
    if (!this.connection) {
      return;
    }
    try {
      const userId = this.authState.user()?.userId;
      if (userId && this.connection.state === signalR.HubConnectionState.Connected) {
        await this.connection.invoke('LeaveGroup', userId);
      }
    } catch {
      /* ignore */
    }
    try {
      await this.connection.stop();
    } catch {
      /* ignore */
    }
    this.connection = null;
    this.connectionState.set('disconnected');
  }

  private async joinUserGroup(): Promise<void> {
    const userId = this.authState.user()?.userId;
    if (!userId || !this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      return;
    }
    try {
      await this.connection.invoke('JoinGroup', userId);
    } catch {
      /* group join failure is non-fatal */
    }
  }
}
