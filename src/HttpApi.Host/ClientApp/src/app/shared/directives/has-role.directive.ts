import { Directive, Input, OnInit, OnDestroy, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

@Directive({
  selector: '[appHasRole]',
  standalone: false
})
export class HasRoleDirective implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private hasView = false;
  private requiredRoles: string[] = [];

  @Input() set appHasRole(roles: string | string[]) {
    this.requiredRoles = Array.isArray(roles) ? roles : [roles];
    this.updateView();
  }

  @Input() set appHasRoleOr(condition: boolean) {
    if (condition) {
      this.showView();
    } else {
      this.updateView();
    }
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.getUser()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateView();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView(): void {
    this.authService.getUser()
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        const hasRole = this.checkRoles(user);
        if (hasRole) {
          this.showView();
        } else {
          this.hideView();
        }
      });
  }

  private checkRoles(user: any): boolean {
    if (!user || this.requiredRoles.length === 0) {
      return false;
    }

    const userRoles = this.extractUserRoles(user);
    return this.requiredRoles.some(role => userRoles.includes(role));
  }

  private extractUserRoles(user: any): string[] {
    const roles: string[] = [];
    
    // Check for roles in different possible locations
    if (user.profile?.role) {
      if (Array.isArray(user.profile.role)) {
        roles.push(...user.profile.role);
      } else {
        roles.push(user.profile.role);
      }
    }
    
    if (user.profile?.roles) {
      if (Array.isArray(user.profile.roles)) {
        roles.push(...user.profile.roles);
      } else {
        roles.push(user.profile.roles);
      }
    }

    // Check for role claim
    if (user.role) {
      if (Array.isArray(user.role)) {
        roles.push(...user.role);
      } else {
        roles.push(user.role);
      }
    }

    // Check for roles claim
    if (user.roles) {
      if (Array.isArray(user.roles)) {
        roles.push(...user.roles);
      } else {
        roles.push(user.roles);
      }
    }

    return roles;
  }

  private showView(): void {
    if (!this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    }
  }

  private hideView(): void {
    if (this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}