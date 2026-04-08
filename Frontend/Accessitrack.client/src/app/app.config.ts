import { ApplicationConfig } from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';
import {
  provideHttpClient,
  withFetch,
  withInterceptors,
} from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { httpErrorInterceptor } from './core/interceptors/http-error.interceptor';

/**
 * Application configuration for Angular 21+.
 * Interceptors are applied in order: auth -> error handling.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withViewTransitions()),
    provideHttpClient(
      withFetch(),
      // Interceptors execute in the order they're provided
      // 1. Auth interceptor adds token to requests
      // 2. Error interceptor handles responses and errors
      withInterceptors([authInterceptor, httpErrorInterceptor])
    ),
  ],
};
