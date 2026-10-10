import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, Subject, catchError } from 'rxjs';

import { AuthenticationService } from '@core/authentication/authentication.service';
import { TokenResponse } from '@core/models/identity/token-response.model';

const AUTH_EXCLUDED_PATHS: string[] = [
  '/auth/login',
  '/auth/verify-otp',
  '/auth/refresh-token',
  '/auth/logout',
];

let isRefreshing: boolean = false;
let refreshSuccess$: Subject<string> | null = null;
let refreshFail$: Subject<Error> | null = null;

export const authenticationInterceptor: HttpInterceptorFn = (request, next) => {
  const authService: AuthenticationService = inject(AuthenticationService);
  const token: string | null = authService.getAccessToken();

  if (token !== null) {
    request = addTokenToRequest(request, token);
  }

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token !== null && !isAuthPath(request.url)) {
        return handle401Error(request, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function isAuthPath(url: string): boolean {
  return AUTH_EXCLUDED_PATHS.some((path) => url.includes(path));
}

function addTokenToRequest<T>(request: HttpRequest<T>, token: string): HttpRequest<T> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function handle401Error<T>(
  request: HttpRequest<T>,
  next: HttpHandlerFn,
  authService: AuthenticationService
): Observable<HttpEvent<T>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshSuccess$ = new Subject<string>();
    refreshFail$ = new Subject<Error>();

    authService.refreshToken().subscribe({
      next: (tokenResponse: TokenResponse): void => {
        isRefreshing = false;
        refreshSuccess$!.next(tokenResponse.accessToken);
        refreshSuccess$!.complete();
        refreshSuccess$ = null;
        refreshFail$ = null;
      },
      error: (error: Error): void => {
        isRefreshing = false;
        refreshFail$!.next(error);
        refreshFail$!.complete();
        refreshFail$ = null;
        refreshSuccess$ = null;
      },
    });
  }

  if (refreshSuccess$ && refreshFail$) {
    return new Observable<HttpEvent<T>>((subscriber) => {
      const successSub = refreshSuccess$!.subscribe({
        next: (newToken: string) => {
          next(addTokenToRequest(request, newToken)).subscribe({
            next: (v) => subscriber.next(v as HttpEvent<T>),
            error: (e) => subscriber.error(e),
            complete: () => subscriber.complete(),
          });
        },
      });
      const failSub = refreshFail$!.subscribe({
        next: (err: Error) => subscriber.error(err),
      });
      return () => {
        successSub.unsubscribe();
        failSub.unsubscribe();
      };
    });
  }

  return throwError(() => new Error('Unexpected refresh state'));
}
