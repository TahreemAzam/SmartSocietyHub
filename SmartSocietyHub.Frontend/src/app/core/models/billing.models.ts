// Mirrors SmartSocietyHub.Application.Features.Billing.DTOs exactly.

export type BillStatus = 'Unpaid' | 'PendingVerification' | 'Paid' | 'Overdue';
export type PaymentStatus = 'Pending' | 'Verified' | 'Rejected';

export interface BillingCharges {
  maintenanceAmount: number;
  securityAmount: number;
  waterAmount: number;
  totalAmount: number;
  updatedAt: string;
}

export interface UpdateBillingChargesRequest {
  maintenanceAmount: number;
  securityAmount: number;
  waterAmount: number;
}

export interface GenerateBillsRequest {
  billingMonth: string;
  dueDate: string;
}

export interface Bill {
  id: string;
  invoiceNumber: string;
  residentId: string;
  residentName: string;
  houseNumber: string;
  block: string;
  billingMonth: string;
  issueDate: string;
  dueDate: string;
  maintenanceAmount: number;
  securityAmount: number;
  waterAmount: number;
  totalAmount: number;
  status: BillStatus;
  createdAt: string;
  updatedAt?: string | null;
}

export interface UpdateBillRequest {
  billingMonth: string;
  dueDate: string;
  maintenanceAmount: number;
  securityAmount: number;
  waterAmount: number;
}

export interface Payment {
  id: string;
  billId: string;
  invoiceNumber: string;
  residentId: string;
  residentName: string;
  amount: number;
  paymentMethod: string;
  paymentProofPath?: string | null;
  status: PaymentStatus;
  submittedAt: string;
  verifiedAt?: string | null;
  rejectionReason?: string | null;
}

export interface VerifyPaymentRequest {
  isApproved: boolean;
  rejectionReason?: string | null;
}
