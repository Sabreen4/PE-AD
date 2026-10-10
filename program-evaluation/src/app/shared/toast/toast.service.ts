import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  leaving: boolean;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  public readonly toasts = signal<Toast[]>([]);

  public show(type: Toast['type'], title: string, message: string, duration = 4000): void {
    const id = ++this.nextId;
    this.toasts.update((list) => [...list, { id, type, title, message, leaving: false }]);
    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }
  }

  public dismiss(id: number): void {
    this.toasts.update((list) => list.map((t) => (t.id === id ? { ...t, leaving: true } : t)));
    setTimeout(() => {
      this.toasts.update((list) => list.filter((t) => t.id !== id));
    }, 300);
  }

  public success(title: string, message: string, duration?: number): void {
    this.show('success', title, message, duration);
  }
  public error(title: string, message: string, duration?: number): void {
    this.show('error', title, message, duration);
  }
  public warning(title: string, message: string, duration?: number): void {
    this.show('warning', title, message, duration);
  }
  public info(title: string, message: string, duration?: number): void {
    this.show('info', title, message, duration);
  }
}
