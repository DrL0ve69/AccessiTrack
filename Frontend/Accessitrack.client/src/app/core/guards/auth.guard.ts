import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Auth Guard: Protects routes that require user authentication.
 * Redirects to login page if user is not authenticated.
 * 
 * Usage: canActivate: [authGuard]
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // User not authenticated, redirect to login
  router.navigate(['/login']);
  return false;
};
