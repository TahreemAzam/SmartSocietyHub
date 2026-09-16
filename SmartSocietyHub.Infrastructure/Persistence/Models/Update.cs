using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Update
    {
        public Guid Id { get; set; }

        // ============================================================
        // COMMON UPDATE INFORMATION
        // ============================================================

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public UpdateType Type { get; set; }

        // ============================================================
        // PUBLISHING
        // ============================================================

        // False when the update is still a draft.
        // True when the Admin has published it.
        public bool IsPublished { get; set; }

        // Set when the update is published.
        public DateTime? PublishedAt { get; set; }

        // ============================================================
        // EVENT INFORMATION
        // ============================================================

        // These fields are null for Announcements.
        // They are required for Events at the service-validation level.
        public DateTime? EventDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string? Location { get; set; }

        // ============================================================
        // AUDIT INFORMATION
        // ============================================================

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}