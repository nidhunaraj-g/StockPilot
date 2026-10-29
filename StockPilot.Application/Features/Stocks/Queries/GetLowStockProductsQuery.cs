using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Stocks.Queries
{
    public record GetLowStockProductsQuery(
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<LowStockProductListResponse>;

    public class LowStockProductResponse
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStockLevel { get; set; }
    }

    public class LowStockProductListResponse
    {
        public List<LowStockProductResponse> Products { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetLowStockProductsQueryHandler(
        IGenericRepository<Product> productRepository)
        : IRequestHandler<GetLowStockProductsQuery, LowStockProductListResponse>
    {
        public async Task<LowStockProductListResponse> Handle(
            GetLowStockProductsQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException("PageNumber must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException("PageSize must be between 1 and 100.");

            var productsQuery = productRepository.GetQueryable()
                .Where(p => p.IsActive &&
                            p.StockQuantity <= p.MinimumStockLevel);

            var totalCount = await productsQuery.CountAsync(cancellationToken);

            var products = await productsQuery
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new LowStockProductResponse
                {
                    ProductId = p.Id,
                    SKU = p.SKU,
                    Name = p.Name,
                    CategoryName = p.Category.Name,
                    StockQuantity = p.StockQuantity,
                    MinimumStockLevel = p.MinimumStockLevel
                })
                .ToListAsync(cancellationToken);

            return new LowStockProductListResponse
            {
                Products = products,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}
