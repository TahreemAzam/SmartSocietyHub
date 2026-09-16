import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { ComplaintApi } from '../../../core/api/complaint.api';
import { StaffApi } from '../../../core/api/staff.api';
import { Complaint } from '../../../core/models/complaint.models';
import { MaintenanceStaff } from '../../../core/models/staff.models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { complaintStatusBadgeClass } from '../../../shared/utils/complaint-status';

const STATUS_FILTERS = ['All', 'Open', 'Assigned', 'InProgress', 'Resolved', 'Closed', 'Reopened'];

interface AssignForm {
  maintenanceStaffUserId: FormControl<string>;
}

@Component({
  selector: 'app-complaints',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './complaints.html',
  styleUrl: './complaints.scss',
})
export class AdminComplaints implements OnInit {
  private readonly complaintApi = inject(ComplaintApi);
  private readonly staffApi = inject(StaffApi);
  private readonly toast = inject(ToastService);

  protected readonly statusFilters = STATUS_FILTERS;
  protected readonly selectedStatus = signal('All');
  protected readonly badgeClass = complaintStatusBadgeClass;

  protected readonly complaints = signal<Complaint[]>([]);
  protected readonly staff = signal<MaintenanceStaff[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly viewingComplaint = signal<Complaint | null>(null);
  protected readonly assigningComplaint = signal<Complaint | null>(null);
  protected readonly assigning = signal(false);
  protected readonly assignError = signal<string | null>(null);

  protected readonly filteredComplaints = computed(() => {
    const status = this.selectedStatus();

    if (status === 'All') {
      return this.complaints();
    }

    return this.complaints().filter((c) => c.status === status);
  });

  protected readonly assignForm = new FormGroup<AssignForm>({
    maintenanceStaffUserId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      complaints: this.complaintApi.getAll(),
      staff: this.staffApi.getAllMaintenanceStaff(),
    }).subscribe({
      next: ({ complaints, staff }) => {
        this.complaints.set(complaints);
        this.staff.set(staff);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load complaints.'));
        this.loading.set(false);
      },
    });
  }

  protected viewComplaint(complaint: Complaint): void {
    this.viewingComplaint.set(complaint);
  }

  protected openAssignForm(complaint: Complaint): void {
    this.assignError.set(null);
    this.assignForm.reset({ maintenanceStaffUserId: '' });
    this.assigningComplaint.set(complaint);
  }

  protected submitAssign(): void {
    const complaint = this.assigningComplaint();

    if (!complaint || this.assignForm.invalid) {
      this.assignForm.markAllAsTouched();
      return;
    }

    this.assignError.set(null);
    this.assigning.set(true);

    this.complaintApi
      .assign(complaint.id, this.assignForm.getRawValue())
      .subscribe({
        next: () => {
          this.assigning.set(false);
          this.assigningComplaint.set(null);
          this.toast.success('Complaint assigned successfully.');
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          this.assigning.set(false);
          this.assignError.set(extractErrorMessage(error, 'Unable to assign this complaint.'));
        },
      });
  }
}
