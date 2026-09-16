using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class FacilityBooking
    {
        public Guid Id { get; set; }

        // Facility being booked
        public Guid FacilityId { get; set; }

        public Facility Facility { get; set; } = null!;

        // Resident making the booking
        public Guid ResidentId { get; set; }

        public Resident Resident { get; set; } = null!;

        // Booking date
        public DateTime BookingDate { get; set; }

        // Booking time
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        // Confirmed, Cancelled, Completed
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}
