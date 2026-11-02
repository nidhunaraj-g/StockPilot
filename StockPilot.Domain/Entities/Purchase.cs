using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class Purchase : AuditableEntity
    {
        public string PurchaseNumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public DateTime PurchaseDateUtc { get; set; }

        public DateTime? ExpectedDeliveryDateUtc { get; set; }

        public DateTime? ReceivedDateUtc { get; set; }

        // Draft, Ordered, PartiallyReceived, Received, Cancelled
        public string Status { get; set; } = "Draft";

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public ICollection<PurchaseItem> Items { get; set; } = [];
    }
}