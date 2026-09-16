import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PropertyApi } from '../../../core/api/property.api';
import { AuthService } from '../../../core/auth/auth.service';
import { PageHeader } from '../../../shared/ui/page-header/page-header';

interface QuickLink {
  label: string;
  description: string;
  path: string;
  icon: string;
}

const QUICK_LINKS: QuickLink[] = [
  {
    label: 'Properties',
    description: 'Manage houses, blocks and occupancy status.',
    path: '/admin/properties',
    icon: 'M5 3h14v18l-2.5-1.5L14 21l-2.5-1.5L9 21l-2-1.5V3',
  },
  {
    label: 'Residents',
    description: 'Manage owners, tenants and family members.',
    path: '/admin/residents',
    icon: 'M3.5 19c0-3 2.5-5.2 5.5-5.2s5.5 2.2 5.5 5.2',
  },
  {
    label: 'Complaints',
    description: 'Review, assign and track resident complaints.',
    path: '/admin/complaints',
    icon: 'M9 4V3a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v1',
  },
  {
    label: 'Maintenance Staff',
    description: 'Create and manage maintenance staff accounts.',
    path: '/admin/staff',
    icon: 'M14.5 3.5a4 4 0 0 0-5 5L4 14l2.5 2.5 5.5-5.5a4 4 0 0 0 5-5l-2.5-2.5z',
  },
  {
    label: 'Facilities & Bookings',
    description: 'Manage facilities and review resident bookings.',
    path: '/admin/facilities',
    icon: 'M4 5h16v16H4z',
  },
  {
    label: 'Billing',
    description: 'Configure charges, generate bills and verify payments.',
    path: '/admin/billing',
    icon: 'M6 3h12v18l-2.5-1.5L13 21l-2.5-1.5L8 21l-2-1.5V3z',
  },
  {
    label: 'Community Updates',
    description: 'Publish announcements and events to the community.',
    path: '/admin/updates',
    icon: 'M3 10v4a1 1 0 0 0 1 1h2l3 5V4l-3 5H4a1 1 0 0 0-1 1z',
  },
];

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink, PageHeader],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard implements OnInit {
  private readonly propertyApi = inject(PropertyApi);
  protected readonly auth = inject(AuthService);

  protected readonly quickLinks = QUICK_LINKS;

  protected readonly totalProperties = signal<number | null>(null);
  protected readonly vacantProperties = signal<number | null>(null);
  protected readonly occupiedProperties = signal<number | null>(null);

  ngOnInit(): void {
    this.propertyApi.getAll().subscribe({
      next: (properties) => {
        this.totalProperties.set(properties.length);
        this.vacantProperties.set(properties.filter((p) => p.status === 'Vacant').length);
        this.occupiedProperties.set(properties.filter((p) => p.status === 'Occupied').length);
      },
      // Dashboard stats are a convenience, not critical — fail quietly and
      // let the stat cards show a dash rather than blocking the page with
      // an error banner for a secondary widget.
      error: () => undefined,
    });
  }
}
