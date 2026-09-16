using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Application.Features.PropertyManagement.DTOs
{
    public class PropertyResponse
    {
        public Guid Id { get; set; }

        public string HouseNumber { get; set; } = string.Empty;

        public string Block { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
