using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Bill
    {
        public Guid Id { get; set; }

        // Unique invoice number
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        // Resident who owns this bill
        public Guid ResidentId { get; set; }

        public Resident Resident { get; set; } = null!;

        // Billing month
        public DateTime BillingMonth { get; set; }

        // Invoice dates
        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        // Monthly charges
        public decimal MaintenanceAmount { get; set; }

        public decimal SecurityAmount { get; set; }

        public decimal WaterAmount { get; set; }

        // Total of all charges
        public decimal TotalAmount { get; set; }

        // Unpaid, PendingVerification, Paid, Overdue
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Unpaid";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}