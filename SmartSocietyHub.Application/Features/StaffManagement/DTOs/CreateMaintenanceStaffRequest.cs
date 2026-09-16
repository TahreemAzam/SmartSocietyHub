using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.StaffManagement.DTOs
{
    public class CreateMaintenanceStaffRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Full name cannot be empty or whitespace."
        )]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Phone number cannot be empty or whitespace."
        )]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}