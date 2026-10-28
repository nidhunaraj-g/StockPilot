using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
        public bool IsActive { get; set; } = true;
    }
}
