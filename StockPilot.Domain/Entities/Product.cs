using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class Product: AuditableEntity
    {
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int MinimumStockLevel { get; set; }
        public byte[] RowVersion { get; set; } = [];
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
        public ICollection<ProductImage> Images { get; set; }= [];
    }
}
