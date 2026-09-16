import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { ComplaintApi } from '../../../core/api/complaint.api';
import { Complaint } from '../../../core/models/complaint.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { complaintStatusBadgeClass } from '../../../shared/utils/complaint-status';

const CATEGORIES = ['Plumbing', 'Electrical', 'Security', 'Cleaning', 'Structural', 'Other'];

interface ComplaintForm {
  title: FormControl<string>;
  description: FormControl<string>;
  category: FormControl<string>;
}

@Component({
  selector: 'app-resident-complaints',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './resident-complaints.html',
  styleUrl: './resident-complaints.scss',
})
export class ResidentComplaints implements OnInit {
  private readonly complaintApi = inject(ComplaintApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly categories = CATEGORIES;
  protected readonly badgeClass = complaintStatusBadgeClass;

  protected readonly complaints = signal<Complaint[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly viewingComplaint = signal<Complaint | null>(null);
  protected readonly showForm = signal(false);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly actioning = signal(false);

  protected readonly form = new FormGroup<ComplaintForm>({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(1000)] }),
    category: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.complaintApi.getMy().subscribe({
      next: (complaints) => {
        this.complaints.set(complaints);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load your complaints.'));
        this.loading.set(false);
      },
    });
  }

  protected openCreateForm(): void {
    this.formError.set(null);
    this.form.reset({ title: '', description: '', category: '' });
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

    this.complaintApi.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success('Complaint submitted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(extractErrorMessage(error, 'Unable to submit this complaint.'));
      },
    });
  }

  protected viewComplaint(complaint: Complaint): void {
    this.viewingComplaint.set(complaint);
  }

  protected async acceptResolution(complaint: Complaint): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Accept resolution',
      message: 'Confirm that this complaint has been resolved to your satisfaction. It will be closed.',
      confirmLabel: 'Yes, Close It',
    });

    if (confirmed) {
      this.submitResidentAction(complaint, true);
    }
  }

  protected async rejectResolution(complaint: Complaint): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Reopen complaint',
      message: 'This will reopen the complaint so maintenance staff can address it again.',
      confirmLabel: 'Reopen',
      danger: true,
    });

    if (confirmed) {
      this.submitResidentAction(complaint, false);
    }
  }

  private submitResidentAction(complaint: Complaint, isSatisfied: boolean): void {
    this.actioning.set(true);

    this.complaintApi.residentAction(complaint.id, { isSatisfied }).subscribe({
      next: () => {
        this.actioning.set(false);
        this.viewingComplaint.set(null);
        this.toast.success(isSatisfied ? 'Complaint closed. Thank you!' : 'Complaint reopened.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.actioning.set(false);
        this.toast.error(extractErrorMessage(error, 'Unable to update this complaint.'));
      },
    });
  }
}
