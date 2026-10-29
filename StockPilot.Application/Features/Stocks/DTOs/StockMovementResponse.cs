using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Stocks.DTOs
{
    public class StockMovementResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
