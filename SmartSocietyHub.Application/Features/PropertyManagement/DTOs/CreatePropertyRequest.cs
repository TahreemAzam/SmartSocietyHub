using System.ComponentModel.DataAnnotations;
using SmartSocietyHub.Application.Features.PropertyManagement.Enums;

namespace SmartSocietyHub.Application.Features.PropertyManagement.DTOs
{
    public class CreatePropertyRequest
    {
        [Required]
        [MaxLength(50)]
        [RegularExpression(@".*\S.*", ErrorMessage = "House number cannot be empty or whitespace.")]
        public string HouseNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression(@".*\S.*", ErrorMessage = "Block cannot be empty or whitespace.")]
        public string Block { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        [RegularExpression(@".*\S.*", ErrorMessage = "Status cannot be empty or whitespace.")]
        [EnumDataType(typeof(PropertyStatus), ErrorMessage = "Status must be either Vacant or Occupied.")]
        public string Status { get; set; } = string.Empty;
    }
}