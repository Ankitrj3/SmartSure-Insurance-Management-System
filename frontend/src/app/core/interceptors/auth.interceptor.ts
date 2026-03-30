import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';

/**
 * HTTP interceptor that:
 *  1. Attaches the stored JWT as a Bearer token on every outgoing request.
 *  2. On 401 responses, attempts a silent token refresh using the refresh token
 *     stored in localStorage (browser cache), then retries the original request.
 *  3. If the refresh itself fails, logs the user out and redirects to /login.
 */
export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const router      = inject(Router);

  // Helper to clone the request with the latest access token
  const withToken = (r: HttpRequest<unknown>, token: string) =>
    r.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

  const token = authService.getToken();

  // Skip attaching Bearer for login / register endpoints
  const isAuthEndpoint = req.url.includes('/auth/login')
                      || req.url.includes('/auth/register')
                      || req.url.includes('/auth/refresh');

  const outgoing = token && !isAuthEndpoint ? withToken(req, token) : req;

  return next(outgoing).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only attempt silent refresh on 401 Unauthorized, and not for auth endpoints
      if (error.status === 401 && !isAuthEndpoint) {
        const refreshToken = authService.getRefreshToken();

        if (!refreshToken) {
          authService.logout();
          router.navigate(['/login']);
          return throwError(() => error);
        }

        // Try to get a new access token using the refresh token from localStorage
        return authService.refreshAccessToken().pipe(
          switchMap(tokens => {
            // Retry the original request with the new access token
            return next(withToken(req, tokens.token));
          }),
          catchError(refreshError => {
            // Refresh token itself is invalid/expired – force logout
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
