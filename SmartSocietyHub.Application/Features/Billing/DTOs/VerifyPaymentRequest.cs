using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class VerifyPaymentRequest
    {
        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}