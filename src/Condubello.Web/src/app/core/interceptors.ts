import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token;
  return next(token && req.url.startsWith('/api') ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req);
};

/** Convierte los errores del envoltorio ApiResponse en un mensaje legible. */
export function mensajeError(e: HttpErrorResponse): string {
  const body = e.error;
  if (body?.errors?.length) return body.errors.map((x: { field: string; message: string }) => x.field ? `${x.field}: ${x.message}` : x.message).join(' · ');
  if (body?.message) return body.message;
  if (e.status === 0) return 'No se pudo conectar con el servidor.';
  return `Error ${e.status}`;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const snack = inject(MatSnackBar);
  return next(req).pipe(
    catchError((e: HttpErrorResponse) => {
      const esLogin = req.url.endsWith('/auth/login');
      if (e.status === 401 && !esLogin) {
        snack.open('Sesión expirada. Inicie sesión de nuevo.', 'OK', { duration: 4000 });
        auth.logout();
      } else if (e.status === 403) {
        snack.open('No tiene permisos para realizar esta acción.', 'OK', { duration: 4000 });
      }
      return throwError(() => e);
    })
  );
};
