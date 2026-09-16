import { Injectable, inject, signal } from '@angular/core';

import { UpdateApi } from '../api/update.api';

// Shared across the sidebar badge and the updates feed page so marking
// something seen on the feed immediately clears the badge, without the
// two having to know about each other directly.
@Injectable({ providedIn: 'root' })
export class UpdatesUnseenStore {
  private readonly updateApi = inject(UpdateApi);

  private readonly countSignal = signal(0);
  readonly count = this.countSignal.asReadonly();

  refresh(): void {
    this.updateApi.getUnseenCount().subscribe({
      next: ({ count }) => this.countSignal.set(count),
      error: () => undefined,
    });
  }
}
