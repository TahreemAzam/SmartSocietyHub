import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  Bill,
  BillingCharges,
  GenerateBillsRequest,
  Payment,
  UpdateBillRequest,
  UpdateBillingChargesRequest,
  VerifyPaymentRequest,
} from '../models/billing.models';

@Injectable({ providedIn: 'root' })
export class BillingApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Billing`;

  // Admin - charges
  getBillingCharges(): Observable<BillingCharges> {
    return this.http.get<BillingCharges>(`${this.baseUrl}/charges`);
  }

  updateBillingCharges(request: UpdateBillingChargesRequest): Observable<BillingCharges> {
    return this.http.put<BillingCharges>(`${this.baseUrl}/charges`, request);
  }

  // Admin - bills
  generateBills(request: GenerateBillsRequest): Observable<Bill[]> {
    return this.http.post<Bill[]>(`${this.baseUrl}/bills`, request);
  }

  getAllBills(): Observable<Bill[]> {
    return this.http.get<Bill[]>(`${this.baseUrl}/bills`);
  }

  updateBill(id: string, request: UpdateBillRequest): Observable<Bill> {
    return this.http.put<Bill>(`${this.baseUrl}/bills/${id}`, request);
  }

  downloadInvoice(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/bills/${id}/invoice`, { responseType: 'blob' });
  }

  // Resident - bills
  getMyBills(): Observable<Bill[]> {
    return this.http.get<Bill[]>(`${this.baseUrl}/my-bills`);
  }

  downloadMyInvoice(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/my-bills/${id}/invoice`, { responseType: 'blob' });
  }

  // Resident - payment
  submitPayment(billId: string, amount: number, proof: File): Observable<Payment> {
    const formData = new FormData();
    formData.append('BillId', billId);
    formData.append('Amount', amount.toString());
    formData.append('PaymentProof', proof);

    return this.http.post<Payment>(`${this.baseUrl}/payments`, formData);
  }

  getMyPayments(): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.baseUrl}/my-payments`);
  }

  // Admin - payment verification
  getPendingPayments(): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.baseUrl}/payments/pending`);
  }

  verifyPayment(paymentId: string, request: VerifyPaymentRequest): Observable<Payment> {
    return this.http.put<Payment>(`${this.baseUrl}/payments/${paymentId}/verify`, request);
  }

  getPaymentProof(paymentId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/payments/${paymentId}/proof`, { responseType: 'blob' });
  }
}
