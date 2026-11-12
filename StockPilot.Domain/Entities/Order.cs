using System;
using System.Collections.Generic;

namespace StockPilot.Domain.Entities
{
    public class Order : BaseEntity
    {
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerPhone { get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryState { get; set; } = string.Empty;
        public string DeliveryCountry { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;

        public DateTime OrderDateUtc { get; set; }

        public string Status { get; set; } = "Pending";

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = [];
    }
}
