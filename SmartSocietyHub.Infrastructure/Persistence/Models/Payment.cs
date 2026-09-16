using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Payment
    {
        public Guid Id { get; set; }

        // Bill against which payment is submitted
        public Guid BillId { get; set; }

        public Bill Bill { get; set; } = null!;

        // Amount paid
        public decimal Amount { get; set; }

        // Offline for now, Online can be added later
        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "Offline";

        // Uploaded screenshot/photo of payment
        [MaxLength(500)]
        public string? PaymentProofPath { get; set; }

        // Pending, Verified, Rejected
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        // When resident submitted the proof
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // When admin verified/rejected it
        public DateTime? VerifiedAt { get; set; }

        // Admin can provide reason if payment proof is rejected
        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}