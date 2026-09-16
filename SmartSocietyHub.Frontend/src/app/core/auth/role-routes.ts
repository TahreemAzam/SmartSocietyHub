import { AppRole } from '../models/auth.models';

// Single source of truth for "where does this role live" so the login
// redirect and the route guards can never disagree with each other.
// SecurityGuard intentionally has no home — that role has no frontend
// surface in the current scope.
const ROLE_HOME_PATHS: Partial<Record<AppRole, string>> = {
  Admin: '/admin',
  Resident: '/resident',
  MaintenanceStaff: '/maintenance',
};

export function getHomePathForRole(role: AppRole | undefined): string {
  if (role && ROLE_HOME_PATHS[role]) {
    return ROLE_HOME_PATHS[role]!;
  }

  // A role with no frontend surface (e.g. SecurityGuard) or an unknown
  // role cannot land anywhere useful — send them back to login rather
  // than into a route that doesn't exist for them.
  return '/login';
}
