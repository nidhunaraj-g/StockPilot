using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.DTOs
{

    public class CreatePurchaseRequest
    {
        public int SupplierId { get; set; }

        public DateTime? ExpectedDeliveryDateUtc { get; set; }

        public decimal TaxAmount { get; set; }

        public string? Notes { get; set; }

        public List<CreatePurchaseItemRequest> Items { get; set; } = [];
    }
}
