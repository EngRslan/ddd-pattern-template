import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError, from } from 'rxjs';
import { catchError, switchMap, take } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isApiUrl = req.url.startsWith(environment.apiUrl) || 
                   req.url.startsWith('/api');
  
  const isAuthUrl = req.url.includes(environment.auth.authority);

  if (!isApiUrl || isAuthUrl) {
    return next(req);
  }

  return from(authService.getAccessToken()).pipe(
    take(1),
    switchMap(token => {
      let authReq = req;
      
      if (token) {
        authReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
      }

      return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            authService.login().catch(loginError => {
              console.error('Failed to redirect to login:', loginError);
              router.navigate(['/not-authorized']);
            });
          } else if (error.status === 403) {
            router.navigate(['/not-authorized']);
          }
          
          return throwError(() => error);
        })
      );
    })
  );
};