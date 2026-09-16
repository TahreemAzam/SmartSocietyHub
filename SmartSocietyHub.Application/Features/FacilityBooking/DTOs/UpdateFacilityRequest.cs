using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.FacilityBooking.DTOs
{
    public class UpdateFacilityRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public TimeSpan OpeningTime { get; set; }

        [Required]
        public TimeSpan ClosingTime { get; set; }

        // Cleaning/Maintenance buffer
        public int BufferMinutes { get; set; } = 60;

        // Admin can activate/deactivate the facility
        public bool IsActive { get; set; } = true;
    }
}