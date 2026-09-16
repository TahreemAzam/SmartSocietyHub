using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class Property
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string HouseNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Block { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}
