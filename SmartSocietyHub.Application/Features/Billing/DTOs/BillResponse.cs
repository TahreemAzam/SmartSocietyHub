using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class BillResponse
    {
        public Guid Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public Guid ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        public string HouseNumber { get; set; } = string.Empty;

        public string Block { get; set; } = string.Empty;

        public DateTime BillingMonth { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public decimal MaintenanceAmount { get; set; }

        public decimal SecurityAmount { get; set; }

        public decimal WaterAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}