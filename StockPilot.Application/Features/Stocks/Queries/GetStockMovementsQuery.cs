using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Stocks.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Stocks.Queries
{
    public record GetStockMovementsQuery(int PageNumber = 1,int PageSize = 10,int? ProductId = null)
    : IRequest<StockMovementListResponse>;

    public class StockMovementListResponse
    {
        public List<StockMovementResponse> Movements { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetStockMovementsQueryHandler(
        IGenericRepository<StockMovement> movementRepository)
        : IRequestHandler<GetStockMovementsQuery, StockMovementListResponse>
    {
        public async Task<StockMovementListResponse> Handle(
            GetStockMovementsQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException("PageNumber must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException("PageSize must be between 1 and 100.");

            if (query.ProductId is <= 0)
                throw new ArgumentException("ProductId must be greater than zero.");

            var movementsQuery = movementRepository.GetQueryable()
                .Include(m => m.Product)
                .Where(m => m.Product.IsActive);

            if (query.ProductId.HasValue)
            {
                movementsQuery = movementsQuery
                    .Where(m => m.ProductId == query.ProductId.Value);
            }

            var totalCount = await movementsQuery.CountAsync(cancellationToken);

            var movements = await movementsQuery
                .OrderByDescending(m => m.CreatedAtUtc)
                .ThenByDescending(m => m.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(m => new StockMovementResponse
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    ProductName = m.Product.Name,
                    Quantity = m.Quantity,
                    MovementType = m.MovementType,
                    Reason = m.Reason,
                    CreatedAtUtc = m.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new StockMovementListResponse
            {
                Movements = movements,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}