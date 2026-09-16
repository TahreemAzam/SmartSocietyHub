using System;

namespace SmartSocietyHub.Application.Features.StaffManagement.DTOs
{
    public class MaintenanceStaffResponse
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
