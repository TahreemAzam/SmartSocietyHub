using System;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class FamilyMember
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
        [MaxLength(30)]
        public string Relationship { get; set; } = string.Empty;

        [Required]
        public Guid ResidentId { get; set; }

        public Resident? Resident { get; set; }
    }
}