using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Resident
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string CNIC { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public Guid PropertyId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        // Links this Resident / Owner to their login account
        // Nullable because existing residents may not have a login yet.
        public Guid? ApplicationUserId { get; set; }

        public Property? Property { get; set; }

        public ICollection<FamilyMember> FamilyMembers { get; set; }
            = new List<FamilyMember>();
    }
}