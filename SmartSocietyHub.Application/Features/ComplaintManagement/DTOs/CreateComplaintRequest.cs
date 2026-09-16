using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Application.Features.ComplaintManagement.DTOs
{
    public class CreateComplaintRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Title cannot be empty or whitespace."
        )]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Description cannot be empty or whitespace."
        )]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Category cannot be empty or whitespace."
        )]
        public string Category { get; set; } = string.Empty;
    }
}

