using System;

namespace SmartSocietyHub.Application.Features.ResidentManagement.DTOs
{
    public class FamilyMemberResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string CNIC { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Relationship { get; set; } = string.Empty;
    }
}