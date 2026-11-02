using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.DTOs
{
    public class PurchaseItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int ReceivedQuantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal Subtotal { get; set; }

        public int PendingQuantity => Quantity - ReceivedQuantity;
    }
}
