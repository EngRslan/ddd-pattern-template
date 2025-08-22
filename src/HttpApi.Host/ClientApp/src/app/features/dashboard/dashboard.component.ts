import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  currentDate = new Date();
  userName: string | null = null;
  userRole: string | null = null;
  sessionExpiry: Date;
  accessLevel = 'Full Access';
  systemStatus = 'healthy';
  
  userPermissions = [
    'Read Entities',
    'Create Entities',
    'Update Entities',
    'Delete Entities',
    'Manage Users',
    'View Reports',
    'System Configuration',
    'Audit Logs'
  ];

  recentActivities = [
    {
      type: 'create',
      description: 'New entity "Customer" created',
      time: '2 minutes ago'
    },
    {
      type: 'update',
      description: 'Domain service "OrderService" updated',
      time: '15 minutes ago'
    },
    {
      type: 'create',
      description: 'Value object "Money" added to domain',
      time: '1 hour ago'
    },
    {
      type: 'delete',
      description: 'Deprecated repository removed',
      time: '3 hours ago'
    },
    {
      type: 'update',
      description: 'Event handler configuration modified',
      time: '5 hours ago'
    }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    // Set session expiry to 1 hour from now for demo
    this.sessionExpiry = new Date();
    this.sessionExpiry.setHours(this.sessionExpiry.getHours() + 1);
  }

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo(): void {
    // Get user info from auth service
    this.authService.getUser()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          if (user) {
            this.userName = user.profile.name || user.profile.preferred_username || 'User';
            this.userRole = this.extractUserRole(user);
            // Update session expiry based on token expiry
            if (user.expires_at) {
              this.sessionExpiry = new Date(user.expires_at * 1000);
            }
          }
        },
        error: (error) => {
          console.error('Error loading user info:', error);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  extractUserRole(user: any): string {
    // Extract role from user claims
    if (user.profile.role) {
      return user.profile.role;
    }
    if (user.profile.roles && user.profile.roles.length > 0) {
      return user.profile.roles[0];
    }
    return 'User';
  }

  logout(): void {
    this.authService.logout();
  }
}
