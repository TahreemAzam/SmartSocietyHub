using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class BillingChargesResponse
    {
        public decimal MaintenanceAmount { get; set; }

        public decimal SecurityAmount { get; set; }

        public decimal WaterAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}