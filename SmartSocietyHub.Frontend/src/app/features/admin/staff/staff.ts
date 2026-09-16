import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { StaffApi } from '../../../core/api/staff.api';
import { CreateMaintenanceStaffRequest, CreateMaintenanceStaffResponse, MaintenanceStaff } from '../../../core/models/staff.models';
import { CredentialsModal } from '../../../shared/ui/credentials-modal/credentials-modal';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';

interface StaffForm {
  fullName: FormControl<string>;
  email: FormControl<string>;
  phoneNumber: FormControl<string>;
}

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [ReactiveFormsModule, PageHeader, EmptyState, Modal, CredentialsModal],
  templateUrl: './staff.html',
  styleUrl: './staff.scss',
})
export class Staff implements OnInit {
  private readonly staffApi = inject(StaffApi);
  private readonly toast = inject(ToastService);

  protected readonly staff = signal<MaintenanceStaff[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly showForm = signal(false);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly newCredentials = signal<CreateMaintenanceStaffResponse | null>(null);

  protected readonly form = new FormGroup<StaffForm>({
    fullName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    phoneNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.staffApi.getAllMaintenanceStaff().subscribe({
      next: (staff) => {
        this.staff.set(staff);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load maintenance staff.'));
        this.loading.set(false);
      },
    });
  }

  protected openCreateForm(): void {
    this.formError.set(null);
    this.form.reset({ fullName: '', email: '', phoneNumber: '' });
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

    this.formError.set(null);
    this.saving.set(true);

    const request: CreateMaintenanceStaffRequest = this.form.getRawValue();

    this.staffApi.createMaintenanceStaff(request).subscribe({
      next: (response) => {
        this.saving.set(false);
        this.showForm.set(false);
        this.newCredentials.set(response);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(extractErrorMessage(error, 'Unable to create this staff account.'));
      },
    });
  }

  protected acknowledgeCredentials(): void {
    this.newCredentials.set(null);
    this.toast.success('Maintenance staff account created successfully.');
  }
}
