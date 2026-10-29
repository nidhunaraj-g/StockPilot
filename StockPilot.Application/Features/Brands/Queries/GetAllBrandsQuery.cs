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
    public record GetAllBrandsQuery(): IRequest<List<BrandResponse>>;

    public class GetAllBrandsQueryHandler(IGenericRepository<Brand> repository,IMapper mapper)
        : IRequestHandler<GetAllBrandsQuery, List<BrandResponse>>
    {
        public async Task<List<BrandResponse>> Handle(GetAllBrandsQuery query,CancellationToken cancellationToken)
        {
            // Get all brands from the database
            var brands = await repository.GetAllAsync(cancellationToken);

            // Filter active brands only
            var activeBrands = brands.Where(brand => brand.IsActive).ToList();

            // Convert entities to response DTOs
            return mapper.Map<List<BrandResponse>>(activeBrands);
        }
    }
}
