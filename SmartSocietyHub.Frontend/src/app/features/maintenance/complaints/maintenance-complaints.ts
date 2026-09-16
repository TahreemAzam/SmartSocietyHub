import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';

import { ComplaintApi } from '../../../core/api/complaint.api';
import { Complaint } from '../../../core/models/complaint.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { complaintStatusBadgeClass } from '../../../shared/utils/complaint-status';

@Component({
  selector: 'app-maintenance-complaints',
  standalone: true,
  imports: [DatePipe, PageHeader, EmptyState, Modal],
  templateUrl: './maintenance-complaints.html',
  styleUrl: './maintenance-complaints.scss',
})
export class MaintenanceComplaints implements OnInit {
  private readonly complaintApi = inject(ComplaintApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly badgeClass = complaintStatusBadgeClass;

  protected readonly complaints = signal<Complaint[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly viewingComplaint = signal<Complaint | null>(null);
  protected readonly updating = signal(false);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.complaintApi.getAssignedToMe().subscribe({
      next: (complaints) => {
        this.complaints.set(complaints);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load your assigned complaints.'));
        this.loading.set(false);
      },
    });
  }

  protected viewComplaint(complaint: Complaint): void {
    this.viewingComplaint.set(complaint);
  }

  protected async startProgress(complaint: Complaint): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Start progress',
      message: `Mark "${complaint.title}" as in progress?`,
      confirmLabel: 'Start',
    });

    if (confirmed) {
      this.updateStatus(complaint, 'InProgress');
    }
  }

  protected async markResolved(complaint: Complaint): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Mark resolved',
      message: `Mark "${complaint.title}" as resolved? The resident will be asked to confirm.`,
      confirmLabel: 'Mark Resolved',
    });

    if (confirmed) {
      this.updateStatus(complaint, 'Resolved');
    }
  }

  private updateStatus(complaint: Complaint, status: 'InProgress' | 'Resolved'): void {
    this.updating.set(true);

    this.complaintApi.updateStatus(complaint.id, { status }).subscribe({
      next: () => {
        this.updating.set(false);
        this.viewingComplaint.set(null);
        this.toast.success('Complaint status updated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.updating.set(false);
        this.toast.error(extractErrorMessage(error, 'Unable to update this complaint.'));
      },
    });
  }
}
