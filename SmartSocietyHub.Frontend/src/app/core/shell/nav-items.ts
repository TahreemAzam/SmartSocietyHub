import { AppRole } from '../models/auth.models';

export type NavIcon =
  | 'home'
  | 'building'
  | 'users'
  | 'clipboard'
  | 'tool'
  | 'calendar'
  | 'receipt'
  | 'megaphone';

export interface NavItem {
  label: string;
  path: string;
  icon: NavIcon;
  exact?: boolean;
  showUnseenBadge?: boolean;
}

const ADMIN_NAV: NavItem[] = [
  { label: 'Dashboard', path: '/admin', icon: 'home', exact: true },
  { label: 'Properties', path: '/admin/properties', icon: 'building' },
  { label: 'Residents', path: '/admin/residents', icon: 'users' },
  { label: 'Complaints', path: '/admin/complaints', icon: 'clipboard' },
  { label: 'Maintenance Staff', path: '/admin/staff', icon: 'tool' },
  { label: 'Facilities & Bookings', path: '/admin/facilities', icon: 'calendar' },
  { label: 'Billing', path: '/admin/billing', icon: 'receipt' },
  { label: 'Community Updates', path: '/admin/updates', icon: 'megaphone' },
];

const RESIDENT_NAV: NavItem[] = [
  { label: 'Dashboard', path: '/resident', icon: 'home', exact: true },
  { label: 'My Complaints', path: '/resident/complaints', icon: 'clipboard' },
  { label: 'Facility Bookings', path: '/resident/bookings', icon: 'calendar' },
  { label: 'Billing & Payments', path: '/resident/billing', icon: 'receipt' },
  { label: 'Community Updates', path: '/resident/updates', icon: 'megaphone', showUnseenBadge: true },
];

const MAINTENANCE_NAV: NavItem[] = [
  { label: 'Dashboard', path: '/maintenance', icon: 'home', exact: true },
  { label: 'Assigned Complaints', path: '/maintenance/complaints', icon: 'clipboard' },
  { label: 'Community Updates', path: '/maintenance/updates', icon: 'megaphone', showUnseenBadge: true },
];

export function getNavItemsForRole(role: AppRole | undefined): NavItem[] {
  switch (role) {
    case 'Admin':
      return ADMIN_NAV;
    case 'Resident':
      return RESIDENT_NAV;
    case 'MaintenanceStaff':
      return MAINTENANCE_NAV;
    default:
      return [];
  }
}
