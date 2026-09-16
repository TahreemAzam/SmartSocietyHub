import { Routes } from '@angular/router';

import { authGuard, guestGuard, roleGuard } from './core/auth/auth.guard';
import { Shell } from './core/shell/shell';
import { Login } from './features/authentication/login/login';

export const routes: Routes = [
  {
    path: 'login',
    component: Login,
    canActivate: [guestGuard],
  },
  {
    path: 'admin',
    component: Shell,
    canActivate: [authGuard, roleGuard(['Admin'])],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/admin/dashboard/admin-dashboard').then((m) => m.AdminDashboard),
      },
      {
        path: 'properties',
        loadComponent: () =>
          import('./features/admin/properties/properties').then((m) => m.Properties),
      },
      {
        path: 'residents',
        loadComponent: () =>
          import('./features/admin/residents/residents').then((m) => m.Residents),
      },
      {
        path: 'complaints',
        loadComponent: () =>
          import('./features/admin/complaints/complaints').then((m) => m.AdminComplaints),
      },
      {
        path: 'staff',
        loadComponent: () => import('./features/admin/staff/staff').then((m) => m.Staff),
      },
      {
        path: 'facilities',
        loadComponent: () =>
          import('./features/admin/facilities/facilities').then((m) => m.Facilities),
      },
      {
        path: 'billing',
        loadComponent: () =>
          import('./features/admin/billing/admin-billing').then((m) => m.AdminBilling),
      },
      {
        path: 'updates',
        loadComponent: () =>
          import('./features/admin/updates/admin-updates').then((m) => m.AdminUpdates),
      },
    ],
  },
  {
    path: 'resident',
    component: Shell,
    canActivate: [authGuard, roleGuard(['Resident'])],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/resident/dashboard/resident-dashboard').then(
            (m) => m.ResidentDashboard,
          ),
      },
      {
        path: 'complaints',
        loadComponent: () =>
          import('./features/resident/complaints/resident-complaints').then(
            (m) => m.ResidentComplaints,
          ),
      },
      {
        path: 'bookings',
        loadComponent: () =>
          import('./features/resident/bookings/resident-bookings').then(
            (m) => m.ResidentBookings,
          ),
      },
      {
        path: 'billing',
        loadComponent: () =>
          import('./features/resident/billing/resident-billing').then((m) => m.ResidentBilling),
      },
      {
        path: 'updates',
        loadComponent: () =>
          import('./features/shared/updates-feed/updates-feed').then((m) => m.UpdatesFeed),
      },
    ],
  },
  {
    path: 'maintenance',
    component: Shell,
    canActivate: [authGuard, roleGuard(['MaintenanceStaff'])],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/maintenance/dashboard/maintenance-dashboard').then(
            (m) => m.MaintenanceDashboard,
          ),
      },
      {
        path: 'complaints',
        loadComponent: () =>
          import('./features/maintenance/complaints/maintenance-complaints').then(
            (m) => m.MaintenanceComplaints,
          ),
      },
      {
        path: 'updates',
        loadComponent: () =>
          import('./features/shared/updates-feed/updates-feed').then((m) => m.UpdatesFeed),
      },
    ],
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
