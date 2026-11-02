using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Purchases.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.Queries
{
    public record GetAllPurchasesQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null,
        string? Status = null
    ) : IRequest<PurchaseListResponse>;

    public class PurchaseListResponse
    {
        public List<PurchaseDto> Purchases { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetAllPurchasesQueryHandler(
        IGenericRepository<Purchase> purchaseRepository)
        : IRequestHandler<GetAllPurchasesQuery, PurchaseListResponse>
    {
        public async Task<PurchaseListResponse> Handle(
            GetAllPurchasesQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException("Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var purchasesQuery = purchaseRepository
                .GetQueryable()
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                purchasesQuery = purchasesQuery.Where(p =>
                    p.PurchaseNumber.Contains(search) ||
                    p.Supplier.SupplierName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = query.Status.Trim();

                purchasesQuery = purchasesQuery.Where(p =>
                    p.Status == status);
            }

            var totalCount = await purchasesQuery.CountAsync(cancellationToken);

            var purchases = await purchasesQuery
                .OrderByDescending(p => p.PurchaseDateUtc)
                .ThenByDescending(p => p.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var purchaseResponses = purchases.Select(purchase => new PurchaseDto
            {
                Id = purchase.Id,
                PurchaseNumber = purchase.PurchaseNumber,
                SupplierId = purchase.SupplierId,
                SupplierName = purchase.Supplier.SupplierName,
                PurchaseDateUtc = purchase.PurchaseDateUtc,
                ExpectedDeliveryDateUtc = purchase.ExpectedDeliveryDateUtc,
                ReceivedDateUtc = purchase.ReceivedDateUtc,
                Status = purchase.Status,
                Subtotal = purchase.Subtotal,
                TaxAmount = purchase.TaxAmount,
                TotalAmount = purchase.TotalAmount,
                Notes = purchase.Notes,

                Items = [.. purchase.Items.Select(item => new PurchaseItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    ReceivedQuantity = item.ReceivedQuantity,
                    UnitCost = item.UnitCost,
                    Subtotal = item.Subtotal
                })]
            }).ToList();

            return new PurchaseListResponse
            {
                Purchases = purchaseResponses,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}
