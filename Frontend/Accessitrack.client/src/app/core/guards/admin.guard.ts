import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Admin Guard: Protects admin-only routes.
 * Requires user to be authenticated AND have Admin role.
 * Redirects to dashboard if user lacks permissions.
 * 
 * Usage: canActivate: [authGuard, adminGuard]
 */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isAdmin()) {
    return true;
  }

  // User not authenticated or not admin, redirect to dashboard
  router.navigate(['/dashboard']);
  return false;
};
