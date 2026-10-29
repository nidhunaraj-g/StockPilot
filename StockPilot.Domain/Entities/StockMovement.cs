using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class StockMovement : BaseEntity
    {
        public int ProductId { get; set; }

        // Stock added or removed.
        public int Quantity { get; set; }

        // IN or OUT
        public string MovementType { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public Product Product { get; set; } = null!;
    }
}
