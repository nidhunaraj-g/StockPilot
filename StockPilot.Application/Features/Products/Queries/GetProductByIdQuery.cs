
using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Products.Queries
{
    public record GetProductByIdQuery(int Id)
        : IRequest<ProductResponse?>;

    public class GetProductByIdQueryHandler(
        IGenericRepository<Product> productRepository,
        IMapper mapper)
        : IRequestHandler<GetProductByIdQuery, ProductResponse?>
    {
        public async Task<ProductResponse?> Handle(
            GetProductByIdQuery query,
            CancellationToken cancellationToken)
        {
            // Load products with Category, Brand, and Images
            var products = await productRepository.GetAllWithIncludesAsync(
                cancellationToken,
                product => product.Category,
                product => product.Brand!,
                product => product.Images);

            // Find the requested active product
            var product = products.FirstOrDefault(
                product => product.Id == query.Id &&
                           product.IsActive);

            if (product is null)
            {
                return null;
            }

            // Map product entity to response DTO
            return mapper.Map<ProductResponse>(product);
        }
    }
}
