import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const authorizedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authorizedReq).pipe(
    catchError((error: unknown) => {
      // Only treat a 401 as "session died" when we actually believed we
      // had one (a token was attached). A 401 on an anonymous request —
      // e.g. wrong password on the login call itself — must be left for
      // the calling component to display inline, not trigger a global
      // logout/redirect that would swallow the error before it's read.
      if (error instanceof HttpErrorResponse && error.status === 401 && token) {
        authService.logout();
      }

      return throwError(() => error);
    }),
  );
};
