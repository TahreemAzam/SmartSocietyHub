using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.FacilityBooking.DTOs
{
    public class BookingResponse
    {
        public Guid Id { get; set; }

        public Guid FacilityId { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public Guid ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}