using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }

        public Guid BillId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public Guid ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string? PaymentProofPath { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? RejectionReason { get; set; }
    }
}
