import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { FacilityBookingApi } from '../../../core/api/facility-booking.api';
import { Booking, CreateFacilityRequest, Facility } from '../../../core/models/facility.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { toApiTimeValue, toTimeInputValue, formatTimeLabel } from '../../../shared/utils/time';

interface FacilityForm {
  name: FormControl<string>;
  description: FormControl<string>;
  location: FormControl<string>;
  openingTime: FormControl<string>;
  closingTime: FormControl<string>;
  bufferMinutes: FormControl<number>;
  isActive: FormControl<boolean>;
}

type Tab = 'facilities' | 'bookings';

@Component({
  selector: 'app-facilities',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './facilities.html',
  styleUrl: './facilities.scss',
})
export class Facilities implements OnInit {
  private readonly facilityBookingApi = inject(FacilityBookingApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly formatTimeLabel = formatTimeLabel;

  protected readonly activeTab = signal<Tab>('facilities');
  protected readonly facilities = signal<Facility[]>([]);
  protected readonly bookings = signal<Booking[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly showForm = signal(false);
  protected readonly editingFacility = signal<Facility | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup<FacilityForm>({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl('', { nonNullable: true }),
    location: new FormControl('', { nonNullable: true }),
    openingTime: new FormControl('08:00', { nonNullable: true, validators: [Validators.required] }),
    closingTime: new FormControl('20:00', { nonNullable: true, validators: [Validators.required] }),
    bufferMinutes: new FormControl(60, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      facilities: this.facilityBookingApi.getAllFacilities(),
      bookings: this.facilityBookingApi.getAllBookings(),
    }).subscribe({
      next: ({ facilities, bookings }) => {
        this.facilities.set(facilities);
        this.bookings.set(
          [...bookings].sort((a, b) => b.bookingDate.localeCompare(a.bookingDate)),
        );
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load facilities.'));
        this.loading.set(false);
      },
    });
  }

  protected openCreateForm(): void {
    this.editingFacility.set(null);
    this.formError.set(null);
    this.form.reset({
      name: '',
      description: '',
      location: '',
      openingTime: '08:00',
      closingTime: '20:00',
      bufferMinutes: 60,
      isActive: true,
    });
    this.showForm.set(true);
  }

  protected openEditForm(facility: Facility): void {
    this.editingFacility.set(facility);
    this.formError.set(null);
    this.form.reset({
      name: facility.name,
      description: facility.description ?? '',
      location: facility.location ?? '',
      openingTime: toTimeInputValue(facility.openingTime),
      closingTime: toTimeInputValue(facility.closingTime),
      bufferMinutes: facility.bufferMinutes,
      isActive: facility.isActive,
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    if (raw.openingTime >= raw.closingTime) {
      this.formError.set('Opening time must be earlier than closing time.');
      return;
    }

    this.formError.set(null);
    this.saving.set(true);

    const request: CreateFacilityRequest = {
      name: raw.name,
      description: raw.description || null,
      location: raw.location || null,
      openingTime: toApiTimeValue(raw.openingTime),
      closingTime: toApiTimeValue(raw.closingTime),
      bufferMinutes: raw.bufferMinutes,
    };

    const editing = this.editingFacility();

    const call = editing
      ? this.facilityBookingApi.updateFacility(editing.id, { ...request, isActive: raw.isActive })
      : this.facilityBookingApi.createFacility(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editing ? 'Facility updated successfully.' : 'Facility created successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(extractErrorMessage(error, 'Unable to save this facility.'));
      },
    });
  }

  protected async deleteFacility(facility: Facility): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Delete facility',
      message: `Delete "${facility.name}"? Existing bookings for this facility will also be removed. This cannot be undone.`,
      confirmLabel: 'Delete',
      danger: true,
    });

    if (!confirmed) {
      return;
    }

    this.facilityBookingApi.deleteFacility(facility.id).subscribe({
      next: () => {
        this.toast.success('Facility deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error, 'Unable to delete this facility.'));
      },
    });
  }
}
