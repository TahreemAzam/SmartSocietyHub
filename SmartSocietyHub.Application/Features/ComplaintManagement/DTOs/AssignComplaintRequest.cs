using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.ComplaintManagement.DTOs
{
    public class AssignComplaintRequest
    {
        [Required]
        public Guid MaintenanceStaffUserId { get; set; }
    }
}