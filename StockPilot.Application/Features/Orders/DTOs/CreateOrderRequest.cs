using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Orders.DTOs
{
    public class CreateOrderRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryState { get; set; } = string.Empty;
        public string DeliveryCountry { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;

        public List<CreateOrderItemRequest> Items { get; set; } = [];
    }
}
