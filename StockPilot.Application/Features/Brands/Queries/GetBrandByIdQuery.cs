using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Brands.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Brands.Queries
{
    public record GetBrandByIdQuery(int Id): IRequest<BrandResponse?>;

    public class GetBrandByIdQueryHandler(IGenericRepository<Brand> repository,IMapper mapper)
        : IRequestHandler<GetBrandByIdQuery, BrandResponse?>
    {
        public async Task<BrandResponse?> Handle(GetBrandByIdQuery query,CancellationToken cancellationToken)
        {
            var brand = await repository.GetByIdAsync(query.Id,cancellationToken);

            // Return active brands only
            if (brand is null || !brand.IsActive)
            {
                return null;
            }

            return mapper.Map<BrandResponse>(brand);
        }
    }
}
