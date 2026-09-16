import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ResidentApi } from '../../../core/api/resident.api';
import { AuthService } from '../../../core/auth/auth.service';
import { Resident } from '../../../core/models/resident.models';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { extractErrorMessage } from '../../../shared/utils/api-error';

interface QuickLink {
  label: string;
  description: string;
  path: string;
  icon: string;
}

const QUICK_LINKS: QuickLink[] = [
  {
    label: 'My Complaints',
    description: 'Raise a new complaint or track an existing one.',
    path: '/resident/complaints',
    icon: 'M9 4V3a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v1',
  },
  {
    label: 'Facility Bookings',
    description: 'Book the pool, courts and other shared facilities.',
    path: '/resident/bookings',
    icon: 'M4 5h16v16H4z',
  },
  {
    label: 'Billing & Payments',
    description: 'View bills, download invoices and submit payments.',
    path: '/resident/billing',
    icon: 'M6 3h12v18l-2.5-1.5L13 21l-2.5-1.5L8 21l-2-1.5V3z',
  },
  {
    label: 'Community Updates',
    description: 'Announcements and events from the management.',
    path: '/resident/updates',
    icon: 'M3 10v4a1 1 0 0 0 1 1h2l3 5V4l-3 5H4a1 1 0 0 0-1 1z',
  },
];

@Component({
  selector: 'app-resident-dashboard',
  standalone: true,
  imports: [RouterLink, PageHeader],
  templateUrl: './resident-dashboard.html',
  styleUrl: './resident-dashboard.scss',
})
export class ResidentDashboard implements OnInit {
  private readonly residentApi = inject(ResidentApi);
  protected readonly auth = inject(AuthService);

  protected readonly quickLinks = QUICK_LINKS;

  protected readonly profile = signal<Resident | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  ngOnInit(): void {
    this.residentApi.getMe().subscribe({
      next: (resident) => {
        this.profile.set(resident);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(
          extractErrorMessage(error, 'Unable to load your resident profile.'),
        );
        this.loading.set(false);
      },
    });
  }
}
