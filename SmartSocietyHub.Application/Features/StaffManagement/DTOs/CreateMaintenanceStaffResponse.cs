using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartSocietyHub.Application.Features.StaffManagement.DTOs
{
    public class CreateMaintenanceStaffResponse
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = "MaintenanceStaff";

        public string TemporaryPassword { get; set; } = string.Empty;
    }
}