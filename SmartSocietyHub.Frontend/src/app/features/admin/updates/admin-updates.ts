import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { UpdateApi } from '../../../core/api/update.api';
import { CommunityUpdate, CreateUpdateRequest, UpdateType } from '../../../core/models/update.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { toApiTimeValue, toTimeInputValue } from '../../../shared/utils/time';

type Filter = 'All' | 'Draft' | 'Published';

interface UpdateForm {
  title: FormControl<string>;
  description: FormControl<string>;
  type: FormControl<UpdateType>;
  eventDate: FormControl<string>;
  startTime: FormControl<string>;
  endTime: FormControl<string>;
  location: FormControl<string>;
}

@Component({
  selector: 'app-admin-updates',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './admin-updates.html',
  styleUrl: './admin-updates.scss',
})
export class AdminUpdates implements OnInit {
  private readonly updateApi = inject(UpdateApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly filters: Filter[] = ['All', 'Draft', 'Published'];
  protected readonly activeFilter = signal<Filter>('All');

  protected readonly updates = signal<CommunityUpdate[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly filteredUpdates = computed(() => {
    const filter = this.activeFilter();

    if (filter === 'All') {
      return this.updates();
    }

    return this.updates().filter((u) =>
      filter === 'Published' ? u.isPublished : !u.isPublished,
    );
  });

  protected readonly showForm = signal(false);
  protected readonly editingUpdate = signal<CommunityUpdate | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup<UpdateForm>({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(2000)] }),
    type: new FormControl<UpdateType>('Announcement', { nonNullable: true, validators: [Validators.required] }),
    eventDate: new FormControl('', { nonNullable: true }),
    startTime: new FormControl('', { nonNullable: true }),
    endTime: new FormControl('', { nonNullable: true }),
    location: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(200)] }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.updateApi.getAll().subscribe({
      next: (updates) => {
        this.updates.set(updates);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load community updates.'));
        this.loading.set(false);
      },
    });
  }

  protected openCreateForm(): void {
    this.editingUpdate.set(null);
    this.formError.set(null);
    this.form.reset({
      title: '',
      description: '',
      type: 'Announcement',
      eventDate: '',
      startTime: '',
      endTime: '',
      location: '',
    });
    this.showForm.set(true);
  }

  protected openEditForm(update: CommunityUpdate): void {
    this.editingUpdate.set(update);
    this.formError.set(null);
    this.form.reset({
      title: update.title,
      description: update.description,
      type: update.type,
      eventDate: update.eventDate?.slice(0, 10) ?? '',
      startTime: toTimeInputValue(update.startTime),
      endTime: toTimeInputValue(update.endTime),
      location: update.location ?? '',
    });
    this.showForm.set(true);
  }

  protected closeForm(): void {
    if (this.saving()) {
      return;
    }

    this.showForm.set(false);
  }

  protected submit(): void {
    const raw = this.form.getRawValue();

    if (raw.type === 'Event') {
      if (!raw.eventDate || !raw.startTime || !raw.endTime || !raw.location.trim()) {
        this.formError.set('Event date, start time, end time and location are all required for an event.');
        return;
      }

      if (raw.startTime >= raw.endTime) {
        this.formError.set('Event end time must be later than start time.');
        return;
      }
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.formError.set(null);
    this.saving.set(true);

    const request: CreateUpdateRequest =
      raw.type === 'Event'
        ? {
            title: raw.title,
            description: raw.description,
            type: 'Event',
            eventDate: raw.eventDate,
            startTime: toApiTimeValue(raw.startTime),
            endTime: toApiTimeValue(raw.endTime),
            location: raw.location,
          }
        : {
            title: raw.title,
            description: raw.description,
            type: 'Announcement',
          };

    const editing = this.editingUpdate();

    const call = editing
      ? this.updateApi.update(editing.id, request)
      : this.updateApi.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editing ? 'Update saved successfully.' : 'Update created as a draft.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(extractErrorMessage(error, 'Unable to save this update.'));
      },
    });
  }

  protected togglePublish(update: CommunityUpdate): void {
    const call = update.isPublished
      ? this.updateApi.unpublish(update.id)
      : this.updateApi.publish(update.id);

    call.subscribe({
      next: () => {
        this.toast.success(update.isPublished ? 'Update unpublished.' : 'Update published to the community.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error, 'Unable to change the publish status.'));
      },
    });
  }

  protected async deleteUpdate(update: CommunityUpdate): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Delete update',
      message: `Delete "${update.title}"? This cannot be undone.`,
      confirmLabel: 'Delete',
      danger: true,
    });

    if (!confirmed) {
      return;
    }

    this.updateApi.delete(update.id).subscribe({
      next: () => {
        this.toast.success('Update deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error, 'Unable to delete this update.'));
      },
    });
  }
}
