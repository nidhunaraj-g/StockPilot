using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.DTOs
{
    public class ReceivePurchaseRequest
    {
        public List<ReceivePurchaseItemRequest> Items { get; set; } = [];
    }

    public class ReceivePurchaseItemRequest
    {
        public int PurchaseItemId { get; set; }

        public int ReceivedQuantity { get; set; }
    }
}
