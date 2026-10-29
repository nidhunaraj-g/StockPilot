using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Products.Commands
{
    public record CreateProductCommand(CreateProductRequest Request)
    : IRequest<ProductResponse>;

    public class CreateProductCommandHandler(IGenericRepository<Product> productRepository,IGenericRepository<Category> categoryRepository,
        IGenericRepository<Brand> brandRepository,IMapper mapper,IValidator<CreateProductRequest> validator)
        : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(CreateProductCommand command,CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(command.Request,cancellationToken);

            var request = command.Request;

            var sku = request.SKU.Trim();
            var barcode = string.IsNullOrWhiteSpace(request.Barcode)
                ? null
                : request.Barcode.Trim();

            var category = await categoryRepository.GetByIdAsync(
                request.CategoryId,
                cancellationToken);

            if (category is null || !category.IsActive)
            {
                throw new ValidationException(
                    "The selected category does not exist or is inactive.");
            }

            if (request.BrandId.HasValue)
            {
                var brand = await brandRepository.GetByIdAsync(request.BrandId.Value,cancellationToken);

                if (brand is null || !brand.IsActive)
                {
                    throw new ValidationException(
                        "The selected brand does not exist or is inactive.");
                }
            }

            var existingSku = await productRepository.FindFirstOrDefaultAsync(product => product.SKU == sku,cancellationToken);

            if (existingSku is not null)
            {
                throw new ValidationException("A product with this SKU already exists.");
            }

            if (barcode is not null)
            {
                var existingBarcode =
                    await productRepository.FindFirstOrDefaultAsync(product => product.Barcode == barcode,cancellationToken);

                if (existingBarcode is not null)
                {
                    throw new ValidationException("A product with this barcode already exists.");
                }
            }

            var product = new Product
            {
                SKU = sku,
                Barcode = barcode,
                Name = request.Name.Trim(),
                Description = request.Description,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                CostPrice = request.CostPrice,
                SellingPrice = request.SellingPrice,
                MinimumStockLevel = request.MinimumStockLevel,
                IsActive = true
            };

            await productRepository.AddAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);

            // Load category and brand navigation properties for the response.
            var savedProduct = await productRepository.GetByIdAsync(
                product.Id,
                cancellationToken);

            // GetByIdAsync may not load navigation properties, so assign
            // the already-validated entities for response mapping.
            product.Category = category;

            if (request.BrandId.HasValue)
            {
                product.Brand = await brandRepository.GetByIdAsync(request.BrandId.Value,cancellationToken);
            }

            return mapper.Map<ProductResponse>(product);
        }
    }
}
