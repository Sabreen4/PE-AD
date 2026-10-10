import { ApplicationConfig, APP_INITIALIZER, ErrorHandler, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { routes } from './app.routes';
import { authenticationInterceptor } from '@core/interceptors/authentication.interceptor';
import { httpErrorInterceptor } from '@core/interceptors/http-error.interceptor';
import { GlobalErrorHandler } from '@core/error-handler/global-error.handler';
import { AuthenticationService } from '@core/authentication/authentication.service';

function initializeAuthFactory(authService: AuthenticationService): () => boolean {
  return (): boolean => authService.tryRestoreSession();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authenticationInterceptor, httpErrorInterceptor])),
    provideAnimations(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAuthFactory,
      deps: [AuthenticationService],
      multi: true,
    },
  ],
};
