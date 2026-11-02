using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }

        public string PurchaseNumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public DateTime PurchaseDateUtc { get; set; }

        public DateTime? ExpectedDeliveryDateUtc { get; set; }

        public DateTime? ReceivedDateUtc { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public List<PurchaseItemDto> Items { get; set; } = [];
    }
}
