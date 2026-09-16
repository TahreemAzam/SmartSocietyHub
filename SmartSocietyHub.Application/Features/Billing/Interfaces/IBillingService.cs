using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartSocietyHub.Application.Features.Billing.DTOs;

namespace SmartSocietyHub.Application.Features.Billing.Interfaces
{
    public interface IBillingService
    {
        // ============================================================
        // ADMIN - BILLING CHARGES / SETTINGS
        // ============================================================

        Task<BillingChargesResponse?> GetBillingChargesAsync();

        Task<BillingChargesResponse> UpdateBillingChargesAsync(
            UpdateBillingChargesRequest request);

        // ============================================================
        // ADMIN - BILL MANAGEMENT
        // ============================================================

        Task<IEnumerable<BillResponse>> GenerateBillsAsync(
            GenerateBillsRequest request);

        Task<IEnumerable<BillResponse>> GetAllBillsAsync();

        Task<BillResponse?> GetBillByIdAsync(
            Guid billId);

        Task<BillResponse?> UpdateBillAsync(
            Guid billId,
            UpdateBillRequest request);

        // ============================================================
        // RESIDENT - BILL MANAGEMENT
        // ============================================================

        Task<IEnumerable<BillResponse>> GetMyBillsAsync(
            Guid applicationUserId);

        Task<BillResponse?> GetMyBillByIdAsync(
            Guid applicationUserId,
            Guid billId);

        // ============================================================
        // INVOICE - ADMIN
        // ============================================================

        Task<byte[]?> GenerateInvoicePdfAsync(
            Guid billId);

        // ============================================================
        // INVOICE - RESIDENT
        // ============================================================

        Task<byte[]?> GenerateMyInvoicePdfAsync(
            Guid applicationUserId,
            Guid billId);

        // ============================================================
        // RESIDENT - PAYMENT
        // ============================================================

        Task<PaymentResponse> SubmitPaymentAsync(
            Guid applicationUserId,
            CreatePaymentRequest request);

        Task<IEnumerable<PaymentResponse>> GetMyPaymentsAsync(
            Guid applicationUserId);

        // ============================================================
        // ADMIN - PAYMENT VERIFICATION
        // ============================================================

        Task<IEnumerable<PaymentResponse>> GetPendingPaymentsAsync();

        Task<PaymentResponse?> GetPaymentByIdAsync(
            Guid paymentId);

        Task<PaymentResponse?> VerifyPaymentAsync(
            Guid paymentId,
            VerifyPaymentRequest request);

        // ============================================================
        // ADMIN - PAYMENT PROOF
        // ============================================================

        Task<string?> GetPaymentProofPathAsync(
            Guid paymentId);
    }
}