using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Products.Queries
{
    public record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null)
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
            if (query.PageNumber < 1)
                throw new ArgumentException(
                    "Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100.");

            var productsQuery = productRepository.GetQueryable()
                .Where(product => product.IsActive)
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                productsQuery = productsQuery.Where(product =>
                    product.Name.Contains(search) ||
                    product.SKU.Contains(search) ||
                    (product.Barcode != null &&
                     product.Barcode.Contains(search)));
            }

            var totalCount = await productsQuery.CountAsync(
                cancellationToken);

            var products = await productsQuery
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new ProductListResponse
            {
                Products = mapper.Map<List<ProductResponse>>(products),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}
