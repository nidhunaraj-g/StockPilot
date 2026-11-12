using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Products.DTOs
{
    public class ProductResponse
    {
        public int Id { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int? BrandId { get; set; }

        public string? BrandName { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public int MinimumStockLevel { get; set; }

        // Current available stock quantity
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public List<ProductImageResponse> Images { get; set; } = [];
    }
}
