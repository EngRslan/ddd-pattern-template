import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, from, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import {
  UserManager,
  UserManagerSettings,
  User,
  WebStorageStateStore,
  Log
} from 'oidc-client-ts';
import { environment } from '../../../environments/environment';

@Injectable()
export class AuthService {
  private userManager: UserManager;
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser$: Observable<User | null>;

  constructor(private router: Router) {
    const settings: UserManagerSettings = {
      authority: environment.auth.authority,
      client_id: environment.auth.clientId,
      redirect_uri: environment.auth.redirectUri,
      post_logout_redirect_uri: environment.auth.postLogoutRedirectUri,
      response_type: environment.auth.responseType,
      scope: environment.auth.scope,
      filterProtocolClaims: true,
      loadUserInfo: environment.auth.loadUserInfo,
      automaticSilentRenew: environment.auth.automaticSilentRenew,
      silent_redirect_uri: environment.auth.silentRedirectUri,
      userStore: new WebStorageStateStore({ store: window.localStorage })
    };

    Log.setLogger(console);
    Log.setLevel(Log.WARN);

    this.userManager = new UserManager(settings);
    this.currentUserSubject = new BehaviorSubject<User | null>(null);
    this.currentUser$ = this.currentUserSubject.asObservable();

    this.userManager.events.addUserLoaded((user) => {
      this.currentUserSubject.next(user);
    });

    this.userManager.events.addUserUnloaded(() => {
      this.currentUserSubject.next(null);
    });

    this.userManager.events.addAccessTokenExpired(() => {
      this.logout();
    });

    this.userManager.events.addSilentRenewError((error) => {
      console.error('Silent renew error:', error);
      this.logout();
    });

    this.loadUser();
  }

  private loadUser(): void {
    this.userManager.getUser().then(user => {
      this.currentUserSubject.next(user);
    }).catch(error => {
      console.error('Error loading user:', error);
    });
  }

  login(): Promise<void> {
    return this.userManager.signinRedirect();
  }

  completeLogin(): Observable<User> {
    return from(this.userManager.signinRedirectCallback()).pipe(
      tap(user => {
        this.currentUserSubject.next(user);
      })
    );
  }

  logout(): Promise<void> {
    return this.userManager.signoutRedirect();
  }

  completeLogout(): Promise<void> {
    return this.userManager.signoutRedirectCallback().then(() => {
      this.currentUserSubject.next(null);
      this.router.navigate(['/']);
    });
  }

  silentRefresh(): Promise<User | null> {
    return this.userManager.signinSilent();
  }

  getUser(): Observable<User | null> {
    return from(this.userManager.getUser());
  }

  getAccessToken(): Observable<string | null> {
    return this.getUser().pipe(
      map(user => user ? user.access_token : null)
    );
  }

  isAuthenticated(): Observable<boolean> {
    return this.getUser().pipe(
      map(user => !!user && !user.expired)
    );
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }
}
