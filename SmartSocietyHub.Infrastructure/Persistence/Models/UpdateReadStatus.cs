using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSocietyHub.Infrastructure.Persistence.Models
{
    public class UpdateReadStatus
    {
        public Guid Id { get; set; }

        public Guid UpdateId { get; set; }

        public Guid UserId { get; set; }

        public DateTime SeenAt { get; set; }

        public Update Update { get; set; } = null!;
    }
}