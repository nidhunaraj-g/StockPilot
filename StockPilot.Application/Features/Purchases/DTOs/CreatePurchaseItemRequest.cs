using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.DTOs
{
    public class CreatePurchaseItemRequest
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }
    }
}
