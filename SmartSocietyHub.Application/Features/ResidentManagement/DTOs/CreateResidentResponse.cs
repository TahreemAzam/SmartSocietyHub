using System;

namespace SmartSocietyHub.Application.Features.ResidentManagement.DTOs
{
    public class CreateResidentResponse
    {
        public ResidentResponse Resident { get; set; }
            = new ResidentResponse();

        public string LoginEmail { get; set; } = string.Empty;

        public string TemporaryPassword { get; set; } = string.Empty;
    }
}