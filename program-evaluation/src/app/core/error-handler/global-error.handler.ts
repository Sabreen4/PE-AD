import { Injectable, ErrorHandler, inject, NgZone } from '@angular/core';
import { ToastService } from '@shared/toast/toast.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly toast = inject(ToastService);
  private readonly ngZone = inject(NgZone);

  handleError(error: Error): void {
    console.error('[GlobalErrorHandler]', error);

    if (error instanceof Error) {
      const msg = error.message || '';
      if (msg.includes('ExpressionChangedAfterItHasBeenCheckedError')) return;
      if (msg.includes('NavigationDuplicated')) return;
      if (msg.startsWith('NG0')) return;

      this.ngZone.run(() => {
        this.toast.error('Application error', 'Something went wrong. Please try again.');
      });
    }
  }
}