import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { PageHeader } from '../../../shared/ui/page-header/page-header';

@Component({
  selector: 'app-maintenance-dashboard',
  standalone: true,
  imports: [RouterLink, PageHeader],
  templateUrl: './maintenance-dashboard.html',
  styleUrl: './maintenance-dashboard.scss',
})
export class MaintenanceDashboard {
  protected readonly auth = inject(AuthService);
}
