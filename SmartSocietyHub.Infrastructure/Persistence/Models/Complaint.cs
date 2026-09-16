using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Complaint
    {
        public Guid Id { get; set; }

        // Resident who submitted the complaint
        public Guid ResidentId { get; set; }

        // Property related to the complaint
        public Guid PropertyId { get; set; }

        // Short title of the complaint
        public string Title { get; set; } = string.Empty;

        // Detailed description of the issue
        public string Description { get; set; } = string.Empty;

        // Complaint category selected by the resident
        public string Category { get; set; } = string.Empty;

        // Open, Assigned, InProgress, Resolved, Closed, Reopened
        public string Status { get; set; } = "Open";

        // Maintenance Staff assigned by Admin
        public Guid? AssignedToUserId { get; set; }

        // Complaint timestamps
        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime? ReopenedAt { get; set; }

        // Navigation properties
        public Resident? Resident { get; set; }

        public Property? Property { get; set; }
    }
}
