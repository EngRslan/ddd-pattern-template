import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, take, catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated().pipe(
    take(1),
    map(isAuthenticated => {
      if (isAuthenticated) {
        return true;
      }
      
      localStorage.setItem('redirectUrl', state.url);
      
      authService.login().catch(error => {
        console.error('Login redirect failed:', error);
        router.navigate(['/unauthorized']);
      });
      
      return false;
    }),
    catchError(() => {
      router.navigate(['/error']);
      return of(false);
    })
  );
};