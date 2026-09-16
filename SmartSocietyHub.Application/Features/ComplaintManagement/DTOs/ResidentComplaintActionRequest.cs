using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.ComplaintManagement.DTOs
{
    public class ResidentComplaintActionRequest
    {
        [Required]
        public bool IsSatisfied { get; set; }
    }
}