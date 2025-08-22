import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: false,
  templateUrl: './auth-callback.component.html',
  styleUrl: './auth-callback.component.scss'
})
export class AuthCallbackComponent implements OnInit {
  error: string | null = null;
  isProcessing = true;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.processAuthCallback();
  }

  private processAuthCallback(): void {
    // Complete the authentication process
    this.authService.completeLogin().subscribe({
      next: (user) => {
        console.log('Authentication successful', user);
        
        // Get the return URL from state or default to dashboard
        const returnUrl = this.getReturnUrl() || '/dashboard';
        
        // Navigate to the return URL
        this.router.navigate([returnUrl]);
      },
      error: (error) => {
        console.error('Authentication error:', error);
        this.error = 'Authentication failed. Please try again.';
        this.isProcessing = false;
        
        // Redirect to home after a delay
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 3000);
      }
    });
  }

  private getReturnUrl(): string | null {
    // Try to get the return URL from session storage
    const returnUrl = sessionStorage.getItem('authReturnUrl');
    if (returnUrl) {
      sessionStorage.removeItem('authReturnUrl');
    }
    return returnUrl;
  }
}