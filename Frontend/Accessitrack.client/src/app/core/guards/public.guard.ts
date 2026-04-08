import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Public Guard: Protects public-only routes (login, register).
 * Prevents authenticated users from accessing these routes.
 * Redirects to dashboard if user is already authenticated.
 * 
 * Usage: canActivate: [publicGuard]
 */
export const publicGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  // User already authenticated, redirect to dashboard
  router.navigate(['/dashboard']);
  return false;
};
