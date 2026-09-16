import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';

import { AuthService } from '../auth/auth.service';
import { BrandMark } from '../../shared/ui/brand-mark/brand-mark';
import { ConfirmService } from '../../shared/ui/confirm-dialog/confirm.service';
import { UpdatesUnseenStore } from '../updates/updates-unseen.store';
import { getNavItemsForRole } from './nav-items';

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Administrator',
  Resident: 'Resident',
  MaintenanceStaff: 'Maintenance Staff',
};

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, BrandMark],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  private readonly confirmService = inject(ConfirmService);
  protected readonly unseenStore = inject(UpdatesUnseenStore);

  protected readonly sidebarOpen = signal(false);

  protected readonly navItems = computed(() => getNavItemsForRole(this.auth.currentUser()?.role));

  protected readonly roleLabel = computed(() => {
    const role = this.auth.currentUser()?.role;
    return role ? ROLE_LABELS[role] ?? role : '';
  });

  protected readonly initials = computed(() => {
    const name = this.auth.currentUser()?.fullName ?? '';
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join('');
  });

  constructor() {
    // Close the mobile drawer automatically whenever navigation completes,
    // and keep the unseen-updates badge fresh as the user navigates
    // around (Admin has no concept of "unseen" updates, so skip them).
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        this.sidebarOpen.set(false);

        if (this.auth.currentUser()?.role !== 'Admin') {
          this.unseenStore.refresh();
        }
      });

    if (this.auth.currentUser()?.role !== 'Admin') {
      this.unseenStore.refresh();
    }
  }

  protected toggleSidebar(): void {
    this.sidebarOpen.update((open) => !open);
  }

  protected async logout(): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Sign out',
      message: 'Are you sure you want to sign out of Horizon Residencia?',
      confirmLabel: 'Sign Out',
    });

    if (confirmed) {
      this.auth.logout();
    }
  }
}
