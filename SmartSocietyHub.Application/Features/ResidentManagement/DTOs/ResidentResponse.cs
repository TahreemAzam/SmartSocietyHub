using System;
using System.Collections.Generic;

namespace SmartSocietyHub.Application.Features.ResidentManagement.DTOs
{
    public class ResidentResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string CNIC { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public Guid PropertyId { get; set; }

        public string HouseNumber { get; set; } = string.Empty;

        public string Block { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        // Automatically calculated from the FamilyMembers list
        public int NumberOfFamilyMembers => FamilyMembers.Count;

        public List<FamilyMemberResponse> FamilyMembers { get; set; }
            = new List<FamilyMemberResponse>();
    }
}