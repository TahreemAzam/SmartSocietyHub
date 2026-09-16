using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.ResidentManagement.DTOs
{
    public class CreateResidentRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Full name cannot be empty or whitespace."
        )]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [RegularExpression(
            @"^\d{5}-\d{7}-\d$",
            ErrorMessage = "CNIC must be in the format 12345-1234567-1."
        )]
        public string CNIC { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Phone number cannot be empty or whitespace."
        )]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Gender cannot be empty or whitespace."
        )]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public Guid PropertyId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        // Number of family members entered in the form
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Number of family members cannot be negative."
        )]
        public int NumberOfFamilyMembers { get; set; }

        // Details of each family member
        public List<FamilyMemberRequest> FamilyMembers { get; set; }
            = new List<FamilyMemberRequest>();
    }
}