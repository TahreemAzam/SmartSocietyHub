using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.ComplaintManagement.DTOs
{
    public class ComplaintResponse
    {
        public Guid Id { get; set; }

        // Resident information
        public Guid ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        // Property information
        public Guid PropertyId { get; set; }

        public string HouseNumber { get; set; } = string.Empty;

        public string Block { get; set; } = string.Empty;

        // Complaint information
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        // Complaint status
        public string Status { get; set; } = string.Empty;

        // Maintenance Staff assignment
        public Guid? AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        // Dates
        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime? ReopenedAt { get; set; }
    }
}