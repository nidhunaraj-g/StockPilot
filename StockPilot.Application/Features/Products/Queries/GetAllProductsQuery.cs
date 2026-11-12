using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace StockPilot.Application.Features.Products.Queries
{
    public record GetAllProductsQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null,
        int? CategoryId = null,
        int? BrandId = null,
        bool? LowStockOnly = null,
        string? SortBy = "Name",
        bool SortDescending = false)
        : IRequest<ProductListResponse>;

    public class ProductListResponse
    {
        public List<ProductResponse> Products { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetAllProductsQueryHandler(
        IGenericRepository<Product> productRepository,
        IMapper mapper)
        : IRequestHandler<GetAllProductsQuery, ProductListResponse>
    {
        public async Task<ProductListResponse> Handle(
            GetAllProductsQuery query,
            CancellationToken cancellationToken)
        {
            // Validate pagination
            if (query.PageNumber < 1)
            {
                throw new ArgumentException(
                    "PageNumber must be at least 1.");
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                throw new ArgumentException(
                    "PageSize must be between 1 and 100.");
            }

            if (query.CategoryId is <= 0)
            {
                throw new ArgumentException(
                    "CategoryId must be greater than zero.");
            }

            if (query.BrandId is <= 0)
            {
                throw new ArgumentException(
                    "BrandId must be greater than zero.");
            }

            // Start with active products
            var productsQuery = productRepository.GetQueryable()
                .Where(p => p.IsActive);

            // Search by product name, SKU, or barcode
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(search) ||
                    p.SKU.Contains(search) ||
                    (p.Barcode != null &&
                     p.Barcode.Contains(search)));
            }

            // Filter by category
            if (query.CategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(
                    p => p.CategoryId == query.CategoryId.Value);
            }

            // Filter by brand
            if (query.BrandId.HasValue)
            {
                productsQuery = productsQuery.Where(
                    p => p.BrandId == query.BrandId.Value);
            }

            // Filter low-stock products
            if (query.LowStockOnly == true)
            {
                productsQuery = productsQuery.Where(
                    p => p.StockQuantity <= p.MinimumStockLevel);
            }

            // Count filtered products before pagination
            var totalCount = await productsQuery.CountAsync(
                cancellationToken);

            // Apply sorting
            productsQuery = (query.SortBy?.Trim().ToLowerInvariant()) switch
            {
                "price" => query.SortDescending
                    ? productsQuery
                        .OrderByDescending(p => p.SellingPrice)
                        .ThenBy(p => p.Id)
                    : productsQuery
                        .OrderBy(p => p.SellingPrice)
                        .ThenBy(p => p.Id),

                "stock" => query.SortDescending
                    ? productsQuery
                        .OrderByDescending(p => p.StockQuantity)
                        .ThenBy(p => p.Id)
                    : productsQuery
                        .OrderBy(p => p.StockQuantity)
                        .ThenBy(p => p.Id),

                "name" or null or "" => query.SortDescending
                    ? productsQuery
                        .OrderByDescending(p => p.Name)
                        .ThenBy(p => p.Id)
                    : productsQuery
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Id),

                _ => throw new ArgumentException(
                    "SortBy must be Name, Price, or Stock.")
            };

            // Load products with Category, Brand, and Images
            var products = await productsQuery
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            // Map entities to response DTOs
            var productResponses = mapper.Map<List<ProductResponse>>(
                products);

            return new ProductListResponse
            {
                Products = productResponses,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}
