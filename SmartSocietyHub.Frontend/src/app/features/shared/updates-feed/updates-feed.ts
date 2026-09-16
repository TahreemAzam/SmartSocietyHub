import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';

import { UpdateApi } from '../../../core/api/update.api';
import { CommunityUpdate } from '../../../core/models/update.models';
import { UpdatesUnseenStore } from '../../../core/updates/updates-unseen.store';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { extractErrorMessage } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-updates-feed',
  standalone: true,
  imports: [DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './updates-feed.html',
  styleUrl: './updates-feed.scss',
})
export class UpdatesFeed implements OnInit {
  private readonly updateApi = inject(UpdateApi);
  private readonly unseenStore = inject(UpdatesUnseenStore);

  protected readonly updates = signal<CommunityUpdate[]>([]);
  protected readonly upcomingEvents = signal<CommunityUpdate[]>([]);
  protected readonly unseenIds = signal<Set<string>>(new Set());
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly viewingUpdate = signal<CommunityUpdate | null>(null);

  protected readonly announcements = computed(() =>
    this.updates().filter((u) => u.type === 'Announcement'),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      updates: this.updateApi.getPublished(),
      upcoming: this.updateApi.getUpcomingEvents(),
      recentUnseen: this.updateApi.getRecentUnseen(),
    }).subscribe({
      next: ({ updates, upcoming, recentUnseen }) => {
        this.updates.set(updates);
        this.upcomingEvents.set(upcoming);
        this.unseenIds.set(new Set(recentUnseen.map((u) => u.id)));
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load community updates.'));
        this.loading.set(false);
      },
    });
  }

  protected isUnseen(update: CommunityUpdate): boolean {
    return this.unseenIds().has(update.id);
  }

  protected viewUpdate(update: CommunityUpdate): void {
    this.viewingUpdate.set(update);

    if (this.isUnseen(update)) {
      this.updateApi.markAsSeen(update.id).subscribe({
        next: () => {
          this.unseenIds.update((ids) => {
            const next = new Set(ids);
            next.delete(update.id);
            return next;
          });
          this.unseenStore.refresh();
        },
        error: () => undefined,
      });
    }
  }
}
