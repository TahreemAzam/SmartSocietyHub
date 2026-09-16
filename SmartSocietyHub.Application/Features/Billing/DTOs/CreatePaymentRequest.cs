using System;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class CreatePaymentRequest
    {
        [Required]
        public Guid BillId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string PaymentProofPath { get; set; } = string.Empty;
    }
}