using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.Updates.DTOs
{
    public class UpdateUpdateRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Expected values:
        // Announcement
        // Event
        public string Type { get; set; } = string.Empty;

        // Required only when Type = Event
        public DateTime? EventDate { get; set; }

        // Required only when Type = Event
        public TimeSpan? StartTime { get; set; }

        // Required only when Type = Event
        public TimeSpan? EndTime { get; set; }

        // Required only when Type = Event
        public string? Location { get; set; }
    }
}
