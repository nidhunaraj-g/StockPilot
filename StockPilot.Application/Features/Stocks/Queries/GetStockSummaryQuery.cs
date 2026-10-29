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
    public record GetStockSummaryQuery()
    : IRequest<StockSummaryResponse>;

    public class StockSummaryResponse
    {
        public int TotalActiveProducts { get; set; }
        public long TotalStockUnits { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
    }

    public class GetStockSummaryQueryHandler(
        IGenericRepository<Product> productRepository)
        : IRequestHandler<GetStockSummaryQuery, StockSummaryResponse>
    {
        public async Task<StockSummaryResponse> Handle(
            GetStockSummaryQuery query,
            CancellationToken cancellationToken)
        {
            var products = productRepository.GetQueryable()
                .Where(p => p.IsActive);

            var summary = new StockSummaryResponse
            {
                TotalActiveProducts = await products.CountAsync(
                    cancellationToken),

                TotalStockUnits = await products
                    .Select(p => (long)p.StockQuantity)
                    .SumAsync(cancellationToken),

                LowStockProducts = await products
                    .CountAsync(
                        p => p.StockQuantity <= p.MinimumStockLevel,
                        cancellationToken),

                OutOfStockProducts = await products
                    .CountAsync(
                        p => p.StockQuantity == 0,
                        cancellationToken)
            };

            return summary;
        }
    }
}
