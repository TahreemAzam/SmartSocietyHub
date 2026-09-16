using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class UpdateBillingChargesRequest
    {
        [Range(0, double.MaxValue)]
        public decimal MaintenanceAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SecurityAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal WaterAmount { get; set; }
    }
}