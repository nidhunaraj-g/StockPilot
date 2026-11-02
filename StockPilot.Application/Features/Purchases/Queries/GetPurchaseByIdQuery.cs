using AutoMapper;
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
    public record GetPurchaseByIdQuery(int Id) : IRequest<PurchaseDto?>;

    public class GetPurchaseByIdQueryHandler(
        IGenericRepository<Purchase> purchaseRepository,
        IMapper mapper)
        : IRequestHandler<GetPurchaseByIdQuery, PurchaseDto?>
    {
        public async Task<PurchaseDto?> Handle(
            GetPurchaseByIdQuery query,
            CancellationToken cancellationToken)
        {
            var purchase = await purchaseRepository
                .GetQueryable()
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(
                    p => p.Id == query.Id && p.IsActive,
                    cancellationToken);

            if (purchase is null)
                return null;

            return mapper.Map<PurchaseDto>(purchase);
        }
    }
}

