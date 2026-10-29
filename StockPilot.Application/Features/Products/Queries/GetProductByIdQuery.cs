using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Products.Queries
{
    public record GetProductByIdQuery(int Id): IRequest<ProductResponse?>;

    public class GetProductByIdQueryHandler(IGenericRepository<Product> productRepository,IMapper mapper)
        : IRequestHandler<GetProductByIdQuery, ProductResponse?>
    {
        public async Task<ProductResponse?> Handle(GetProductByIdQuery query,CancellationToken cancellationToken)
        {
            var products = await productRepository.GetAllWithIncludesAsync(
                cancellationToken,
                product => product.Category,
                product => product.Brand!);

            var product = products.FirstOrDefault(
                product => product.Id == query.Id && product.IsActive);

            if (product is null)
                return null;

            return mapper.Map<ProductResponse>(product);
        }
    }
}