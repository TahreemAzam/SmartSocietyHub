using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class BillingSettings
    {
        public Guid Id { get; set; }

        // Standard monthly maintenance charge
        public decimal MaintenanceAmount { get; set; }

        // Standard monthly security charge
        public decimal SecurityAmount { get; set; }

        // Standard monthly water charge
        public decimal WaterAmount { get; set; }

        // Last time the billing charges were updated
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
