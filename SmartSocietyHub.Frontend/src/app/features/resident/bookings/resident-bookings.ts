import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { FacilityBookingApi } from '../../../core/api/facility-booking.api';
import { Booking, CreateBookingRequest, Facility } from '../../../core/models/facility.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { formatTimeLabel, toApiTimeValue, toTimeInputValue } from '../../../shared/utils/time';

interface BookingForm {
  facilityId: FormControl<string>;
  bookingDate: FormControl<string>;
  startTime: FormControl<string>;
  endTime: FormControl<string>;
}

function todayIsoDate(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-resident-bookings',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './resident-bookings.html',
  styleUrl: './resident-bookings.scss',
})
export class ResidentBookings implements OnInit {
  private readonly facilityBookingApi = inject(FacilityBookingApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly formatTimeLabel = formatTimeLabel;
  protected readonly today = todayIsoDate();

  protected readonly facilities = signal<Facility[]>([]);
  protected readonly bookings = signal<Booking[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly activeFacilities = computed(() =>
    this.facilities().filter((f) => f.isActive),
  );

  protected readonly showForm = signal(false);
  protected readonly editingBooking = signal<Booking | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup<BookingForm>({
    facilityId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    bookingDate: new FormControl(this.today, { nonNullable: true, validators: [Validators.required] }),
    startTime: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    endTime: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly selectedFacility = computed(() => {
    const id = this.form.controls.facilityId.value;
    return this.facilities().find((f) => f.id === id) ?? null;
  });

  ngOnInit(): void {
    this.load();

    this.form.controls.facilityId.valueChanges.subscribe(() => {
      // Re-run validation on time fields since the valid range depends
      // on which facility is selected.
      this.form.controls.startTime.updateValueAndValidity();
      this.form.controls.endTime.updateValueAndValidity();
    });
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      facilities: this.facilityBookingApi.getAllFacilities(),
      bookings: this.facilityBookingApi.getMyBookings(),
    }).subscribe({
      next: ({ facilities, bookings }) => {
        this.facilities.set(facilities);
        this.bookings.set(bookings);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load facilities.'));
        this.loading.set(false);
      },
    });
  }

  protected openBookingForm(facility?: Facility): void {
    this.editingBooking.set(null);
    this.formError.set(null);
    this.form.reset({
      facilityId: facility?.id ?? '',
      bookingDate: this.today,
      startTime: '',
      endTime: '',
    });
    this.showForm.set(true);
  }

  protected openEditForm(booking: Booking): void {
    this.editingBooking.set(booking);
    this.formError.set(null);
    this.form.reset({
      facilityId: booking.facilityId,
      bookingDate: booking.bookingDate.slice(0, 10),
      startTime: toTimeInputValue(booking.startTime),
      endTime: toTimeInputValue(booking.endTime),
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
    const facility = this.selectedFacility();

    if (raw.startTime >= raw.endTime) {
      this.formError.set('Start time must be earlier than end time.');
      return;
    }

    if (facility) {
      const opening = toTimeInputValue(facility.openingTime);
      const closing = toTimeInputValue(facility.closingTime);

      if (raw.startTime < opening || raw.endTime > closing) {
        this.formError.set(
          `This facility is only available between ${formatTimeLabel(facility.openingTime)} and ${formatTimeLabel(facility.closingTime)}.`,
        );
        return;
      }
    }

    this.formError.set(null);
    this.saving.set(true);

    const request: CreateBookingRequest = {
      facilityId: raw.facilityId,
      bookingDate: raw.bookingDate,
      startTime: toApiTimeValue(raw.startTime),
      endTime: toApiTimeValue(raw.endTime),
    };

    const editing = this.editingBooking();

    const call = editing
      ? this.facilityBookingApi.updateMyBooking(editing.id, request)
      : this.facilityBookingApi.createBooking(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editing ? 'Booking updated successfully.' : 'Booking confirmed successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(extractErrorMessage(error, 'Unable to save this booking.'));
      },
    });
  }

  protected async cancelBooking(booking: Booking): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Cancel booking',
      message: `Cancel your booking for ${booking.facilityName}?`,
      confirmLabel: 'Cancel Booking',
      danger: true,
    });

    if (!confirmed) {
      return;
    }

    this.facilityBookingApi.cancelMyBooking(booking.id).subscribe({
      next: () => {
        this.toast.success('Booking cancelled successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error, 'Unable to cancel this booking.'));
      },
    });
  }
}
