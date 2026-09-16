using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class UpdateBillRequest
    {
        [Required]
        public DateTime BillingMonth { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaintenanceAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SecurityAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal WaterAmount { get; set; }
    }
}