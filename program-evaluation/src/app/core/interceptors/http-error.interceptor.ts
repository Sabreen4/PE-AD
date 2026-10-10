import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '@shared/toast/toast.service';

const AUTH_PATHS = ['/auth/login', '/auth/verify-otp', '/auth/refresh-token', '/auth/logout'];

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const url = req.url;

      if (error.status === 0) {
        toast.error('Network error', 'Unable to connect to the server. Please check your connection.');
      } else if (error.status === 401 && !AUTH_PATHS.some((p) => url.includes(p))) {
        toast.error('Session expired', 'Your session has expired. Please log in again.');
        router.navigate(['/auth/login']);
      } else if (error.status === 403) {
        toast.warning('Access denied', 'You do not have permission to perform this action.');
        router.navigate(['/access-denied']);
      } else if (error.status === 500) {
        toast.error('Server error', 'An unexpected server error occurred. Please try again later.');
      }

      return throwError(() => error);
    })
  );
};
