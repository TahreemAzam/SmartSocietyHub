using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Facility
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public TimeSpan OpeningTime { get; set; }

        public TimeSpan ClosingTime { get; set; }

        // Cleaning/Maintenance buffer after every booking
        // Default: 60 minutes
        public int BufferMinutes { get; set; } = 60;

        // Admin can temporarily deactivate a facility
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<FacilityBooking> Bookings { get; set; }
            = new List<FacilityBooking>();
    }
}