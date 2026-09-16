import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AppRole } from '../models/auth.models';
import { AuthService } from './auth.service';
import { getHomePathForRole } from './role-routes';

// Blocks any route that requires an authenticated session.
// Unauthenticated visitors are sent to /login with a returnUrl so they
// land back where they intended once they sign in.
export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

// Keeps an already-authenticated user off the login screen — landing
// them straight on their own dashboard instead of a login form they
// don't need.
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree([getHomePathForRole(authService.currentUser()?.role)]);
};

// Restricts a route branch to a specific set of roles. A logged-in user
// of the wrong role is redirected to *their own* home rather than to
// login (they are authenticated — just not authorized for this branch).
export function roleGuard(allowedRoles: AppRole[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const user = authService.currentUser();

    if (user && allowedRoles.includes(user.role)) {
      return true;
    }

    return router.createUrlTree([getHomePathForRole(user?.role)]);
  };
}
