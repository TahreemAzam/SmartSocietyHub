import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';

import { BillingApi } from '../../../core/api/billing.api';
import { Bill, Payment } from '../../../core/models/billing.models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { downloadBlob } from '../../../shared/utils/download-blob';

const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png'];
const MAX_FILE_SIZE = 5 * 1024 * 1024;

@Component({
  selector: 'app-resident-billing',
  standalone: true,
  imports: [DatePipe, DecimalPipe, PageHeader, EmptyState, Modal],
  templateUrl: './resident-billing.html',
  styleUrl: './resident-billing.scss',
})
export class ResidentBilling implements OnInit {
  private readonly billingApi = inject(BillingApi);
  private readonly toast = inject(ToastService);

  protected readonly bills = signal<Bill[]>([]);
  protected readonly payments = signal<Payment[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly downloadingId = signal<string | null>(null);

  protected readonly payingBill = signal<Bill | null>(null);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly fileError = signal<string | null>(null);
  protected readonly submitError = signal<string | null>(null);
  protected readonly submitting = signal(false);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      bills: this.billingApi.getMyBills(),
      payments: this.billingApi.getMyPayments(),
    }).subscribe({
      next: ({ bills, payments }) => {
        this.bills.set(bills);
        this.payments.set(payments);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load your bills.'));
        this.loading.set(false);
      },
    });
  }

  protected downloadInvoice(bill: Bill): void {
    this.downloadingId.set(bill.id);

    this.billingApi.downloadMyInvoice(bill.id).subscribe({
      next: (blob) => {
        this.downloadingId.set(null);
        downloadBlob(blob, `${bill.invoiceNumber}.pdf`);
      },
      error: (error: HttpErrorResponse) => {
        this.downloadingId.set(null);
        this.toast.error(extractErrorMessage(error, 'Unable to download this invoice.'));
      },
    });
  }

  protected openPaymentForm(bill: Bill): void {
    this.payingBill.set(bill);
    this.selectedFile.set(null);
    this.fileError.set(null);
    this.submitError.set(null);
  }

  protected closePaymentForm(): void {
    if (this.submitting()) {
      return;
    }

    this.payingBill.set(null);
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedFile.set(null);
      return;
    }

    if (!ALLOWED_TYPES.includes(file.type)) {
      this.fileError.set('Only JPG and PNG images are allowed.');
      this.selectedFile.set(null);
      input.value = '';
      return;
    }

    if (file.size > MAX_FILE_SIZE) {
      this.fileError.set('The image must be 5 MB or smaller.');
      this.selectedFile.set(null);
      input.value = '';
      return;
    }

    this.fileError.set(null);
    this.selectedFile.set(file);
  }

  protected submitPayment(): void {
    const bill = this.payingBill();
    const file = this.selectedFile();

    if (!bill) {
      return;
    }

    if (!file) {
      this.fileError.set('Please attach a photo or screenshot of your payment receipt.');
      return;
    }

    this.submitError.set(null);
    this.submitting.set(true);

    this.billingApi.submitPayment(bill.id, bill.totalAmount, file).subscribe({
      next: () => {
        this.submitting.set(false);
        this.payingBill.set(null);
        this.toast.success('Payment submitted. It will be reviewed by the management shortly.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.submitError.set(extractErrorMessage(error, 'Unable to submit this payment.'));
      },
    });
  }
}
