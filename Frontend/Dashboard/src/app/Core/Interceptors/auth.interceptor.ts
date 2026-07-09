import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../../Features/Auth/Services/auth.service';

/**
 * Attaches the JWT access token as `Authorization: Bearer <token>` on every request.
 * On a 401 for a non-auth endpoint, attempts a single refresh + retry; if refresh
 * fails, logs out and routes home.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getToken();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // Don't try to refresh on the auth endpoints themselves (login/refresh/logout etc.),
      // and only react to 401s.
      if (err.status !== 401 || req.url.includes('/auth/')) {
        return throwError(() => err);
      }

      return auth.refresh().pipe(
        switchMap((res) =>
          next(req.clone({ setHeaders: { Authorization: `Bearer ${res.token}` } }))
        ),
        catchError((refreshErr) => {
          auth.logout();
          router.navigateByUrl('/');
          return throwError(() => refreshErr);
        })
      );
    })
  );
};
