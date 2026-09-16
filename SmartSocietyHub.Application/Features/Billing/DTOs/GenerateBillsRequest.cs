using System;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.Billing.DTOs
{
    public class GenerateBillsRequest
    {
        [Required]
        public DateTime BillingMonth { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}