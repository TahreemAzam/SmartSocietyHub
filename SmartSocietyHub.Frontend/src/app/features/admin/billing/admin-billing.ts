import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { BillingApi } from '../../../core/api/billing.api';
import { Bill, BillingCharges, Payment } from '../../../core/models/billing.models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { downloadBlob } from '../../../shared/utils/download-blob';

type Tab = 'charges' | 'bills' | 'payments';

interface ChargesForm {
  maintenanceAmount: FormControl<number>;
  securityAmount: FormControl<number>;
  waterAmount: FormControl<number>;
}

interface GenerateForm {
  billingMonth: FormControl<string>;
  dueDate: FormControl<string>;
}

interface EditBillForm {
  billingMonth: FormControl<string>;
  dueDate: FormControl<string>;
  maintenanceAmount: FormControl<number>;
  securityAmount: FormControl<number>;
  waterAmount: FormControl<number>;
}

interface RejectForm {
  rejectionReason: FormControl<string>;
}

@Component({
  selector: 'app-admin-billing',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe, PageHeader, EmptyState, Modal],
  templateUrl: './admin-billing.html',
  styleUrl: './admin-billing.scss',
})
export class AdminBilling implements OnInit {
  private readonly billingApi = inject(BillingApi);
  private readonly toast = inject(ToastService);

  protected readonly activeTab = signal<Tab>('charges');

  protected readonly charges = signal<BillingCharges | null>(null);
  protected readonly chargesConfigured = signal(true);
  protected readonly bills = signal<Bill[]>([]);
  protected readonly pendingPayments = signal<Payment[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly savingCharges = signal(false);
  protected readonly chargesForm = new FormGroup<ChargesForm>({
    maintenanceAmount: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    securityAmount: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    waterAmount: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
  });

  protected readonly chargesTotal = computed(() => {
    const raw = this.chargesForm.getRawValue();
    return (raw.maintenanceAmount || 0) + (raw.securityAmount || 0) + (raw.waterAmount || 0);
  });

  protected readonly showGenerateForm = signal(false);
  protected readonly generating = signal(false);
  protected readonly generateError = signal<string | null>(null);
  protected readonly generateForm = new FormGroup<GenerateForm>({
    billingMonth: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    dueDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly editingBill = signal<Bill | null>(null);
  protected readonly savingBill = signal(false);
  protected readonly editBillError = signal<string | null>(null);
  protected readonly editBillForm = new FormGroup<EditBillForm>({
    billingMonth: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    dueDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    maintenanceAmount: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    securityAmount: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    waterAmount: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
  });

  protected readonly downloadingId = signal<string | null>(null);

  protected readonly reviewingPayment = signal<Payment | null>(null);
  protected readonly proofImageUrl = signal<string | null>(null);
  protected readonly loadingProof = signal(false);
  protected readonly reviewing = signal(false);
  protected readonly showRejectForm = signal(false);
  protected readonly rejectForm = new FormGroup<RejectForm>({
    rejectionReason: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.billingApi.getBillingCharges().subscribe({
      next: (charges) => {
        this.charges.set(charges);
        this.chargesConfigured.set(true);
        this.chargesForm.reset({
          maintenanceAmount: charges.maintenanceAmount,
          securityAmount: charges.securityAmount,
          waterAmount: charges.waterAmount,
        });
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 404) {
          this.chargesConfigured.set(false);
        }
      },
    });

    this.billingApi.getAllBills().subscribe({
      next: (bills) => {
        this.bills.set(bills);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load bills.'));
        this.loading.set(false);
      },
    });

    this.loadPendingPayments();
  }

  private loadPendingPayments(): void {
    this.billingApi.getPendingPayments().subscribe({
      next: (payments) => this.pendingPayments.set(payments),
      error: () => undefined,
    });
  }

  protected saveCharges(): void {
    if (this.chargesForm.invalid) {
      this.chargesForm.markAllAsTouched();
      return;
    }

    this.savingCharges.set(true);

    this.billingApi.updateBillingCharges(this.chargesForm.getRawValue()).subscribe({
      next: (charges) => {
        this.savingCharges.set(false);
        this.charges.set(charges);
        this.chargesConfigured.set(true);
        this.toast.success('Billing charges updated successfully.');
      },
      error: (error: HttpErrorResponse) => {
        this.savingCharges.set(false);
        this.toast.error(extractErrorMessage(error, 'Unable to update billing charges.'));
      },
    });
  }

  protected openGenerateForm(): void {
    this.generateError.set(null);
    this.generateForm.reset({ billingMonth: '', dueDate: '' });
    this.showGenerateForm.set(true);
  }

  protected submitGenerate(): void {
    if (this.generateForm.invalid) {
      this.generateForm.markAllAsTouched();
      return;
    }

    const raw = this.generateForm.getRawValue();
    this.generateError.set(null);
    this.generating.set(true);

    this.billingApi
      .generateBills({
        billingMonth: `${raw.billingMonth}-01`,
        dueDate: raw.dueDate,
      })
      .subscribe({
        next: (bills) => {
          this.generating.set(false);
          this.showGenerateForm.set(false);
          this.toast.success(`Generated ${bills.length} bill(s) successfully.`);
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          this.generating.set(false);
          this.generateError.set(extractErrorMessage(error, 'Unable to generate bills.'));
        },
      });
  }

  protected openEditBill(bill: Bill): void {
    this.editBillError.set(null);
    this.editingBill.set(bill);
    this.editBillForm.reset({
      billingMonth: bill.billingMonth.slice(0, 7),
      dueDate: bill.dueDate.slice(0, 10),
      maintenanceAmount: bill.maintenanceAmount,
      securityAmount: bill.securityAmount,
      waterAmount: bill.waterAmount,
    });
  }

  protected submitEditBill(): void {
    const bill = this.editingBill();

    if (!bill || this.editBillForm.invalid) {
      this.editBillForm.markAllAsTouched();
      return;
    }

    const raw = this.editBillForm.getRawValue();
    this.editBillError.set(null);
    this.savingBill.set(true);

    this.billingApi
      .updateBill(bill.id, {
        billingMonth: `${raw.billingMonth}-01`,
        dueDate: raw.dueDate,
        maintenanceAmount: raw.maintenanceAmount,
        securityAmount: raw.securityAmount,
        waterAmount: raw.waterAmount,
      })
      .subscribe({
        next: () => {
          this.savingBill.set(false);
          this.editingBill.set(null);
          this.toast.success('Bill updated successfully.');
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          this.savingBill.set(false);
          this.editBillError.set(extractErrorMessage(error, 'Unable to update this bill.'));
        },
      });
  }

  protected downloadInvoice(bill: Bill): void {
    this.downloadingId.set(bill.id);

    this.billingApi.downloadInvoice(bill.id).subscribe({
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

  protected reviewPayment(payment: Payment): void {
    this.reviewingPayment.set(payment);
    this.showRejectForm.set(false);
    this.rejectForm.reset({ rejectionReason: '' });
    this.proofImageUrl.set(null);
    this.loadingProof.set(true);

    this.billingApi.getPaymentProof(payment.id).subscribe({
      next: (blob) => {
        this.loadingProof.set(false);
        this.proofImageUrl.set(URL.createObjectURL(blob));
      },
      error: () => {
        this.loadingProof.set(false);
      },
    });
  }

  protected closeReview(): void {
    const url = this.proofImageUrl();

    if (url) {
      URL.revokeObjectURL(url);
    }

    this.reviewingPayment.set(null);
    this.proofImageUrl.set(null);
  }

  protected approvePayment(): void {
    const payment = this.reviewingPayment();

    if (!payment) {
      return;
    }

    this.reviewing.set(true);

    this.billingApi.verifyPayment(payment.id, { isApproved: true }).subscribe({
      next: () => {
        this.reviewing.set(false);
        this.closeReview();
        this.toast.success('Payment verified. Bill marked as Paid.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.reviewing.set(false);
        this.toast.error(extractErrorMessage(error, 'Unable to verify this payment.'));
      },
    });
  }

  protected submitRejection(): void {
    const payment = this.reviewingPayment();

    if (!payment || this.rejectForm.invalid) {
      this.rejectForm.markAllAsTouched();
      return;
    }

    this.reviewing.set(true);

    this.billingApi
      .verifyPayment(payment.id, {
        isApproved: false,
        rejectionReason: this.rejectForm.getRawValue().rejectionReason,
      })
      .subscribe({
        next: () => {
          this.reviewing.set(false);
          this.closeReview();
          this.toast.success('Payment rejected.');
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          this.reviewing.set(false);
          this.toast.error(extractErrorMessage(error, 'Unable to reject this payment.'));
        },
      });
  }
}
