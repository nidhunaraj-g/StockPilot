using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class PurchaseItem : BaseEntity
    {
        public int PurchaseId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public int ReceivedQuantity { get; set; } = 0;

        public decimal UnitCost { get; set; }

        public decimal Subtotal { get; set; }

        public Purchase Purchase { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
