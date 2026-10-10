import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class SignalrNotificationService {
  private connection: signalR.HubConnection | null = null;

  startConnection(hubUrl: string): void {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.start().catch((err: Error) => console.error('SignalR connection error:', err));
  }

  stopConnection(): void {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
    }
  }

  onNotificationReceived(callback: (notification: Record<string, string | number | boolean | null>) => void): void {
    if (this.connection) {
      this.connection.on('ReceiveNotification', callback);
    }
  }
}
