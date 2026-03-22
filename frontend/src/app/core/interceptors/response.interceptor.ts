import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

/**
 * Response interceptor to handle API response wrapper
 * Unwraps ApiResponse { success, data, message, errors }
 * and returns just the data for easier consumption in components
 */
export const responseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      // Only process HttpResponse events, pass everything else through
      if (event instanceof HttpResponse) {
        const body = event.body;
        // If response has ApiResponse structure, extract the data
        if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
          console.debug('Unwrapping ApiResponse:', body);
          return event.clone({ body: body.data ?? body });
        }
      }
      return event;
    })
  );
};
