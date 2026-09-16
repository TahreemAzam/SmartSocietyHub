// Mirrors SmartSocietyHub.Application.Features.Authentication.DTOs exactly.
// Do not add fields the backend does not actually return.

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  role: AppRole;
}

// The four roles seeded/assignable in SmartSocietyHub.Infrastructure.Seed.IdentitySeeder.
// SecurityGuard has no frontend surface yet (out of current scope) but the value can
// still arrive from the backend, so it stays in the type.
export type AppRole = 'Admin' | 'Resident' | 'SecurityGuard' | 'MaintenanceStaff';

export interface AuthenticatedUser {
  userId: string;
  fullName: string;
  email: string;
  role: AppRole;
}
