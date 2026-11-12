
using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Products.Commands
{
    public record CreateProductCommand(CreateProductRequest Request)
        : IRequest<ProductResponse>;

    public class CreateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<Brand> brandRepository,
        IMapper mapper,
        IValidator<CreateProductRequest> validator)
        : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(
            CreateProductCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            var request = command.Request;

            var sku = request.SKU.Trim();

            var barcode = string.IsNullOrWhiteSpace(request.Barcode)
                ? null
                : request.Barcode.Trim();

            // Validate category
            var category = await categoryRepository.GetByIdAsync(
                request.CategoryId,
                cancellationToken);

            if (category is null || !category.IsActive)
            {
                throw new ValidationException(
                    "The selected category does not exist or is inactive.");
            }

            // Validate brand
            Brand? brand = null;

            if (request.BrandId.HasValue)
            {
                brand = await brandRepository.GetByIdAsync(
                    request.BrandId.Value,
                    cancellationToken);

                if (brand is null || !brand.IsActive)
                {
                    throw new ValidationException(
                        "The selected brand does not exist or is inactive.");
                }
            }

            // Check duplicate SKU
            var existingSku =
                await productRepository.FindFirstOrDefaultAsync(
                    product => product.SKU == sku,
                    cancellationToken);

            if (existingSku is not null)
            {
                throw new ValidationException(
                    "A product with this SKU already exists.");
            }

            // Check duplicate barcode
            if (barcode is not null)
            {
                var existingBarcode =
                    await productRepository.FindFirstOrDefaultAsync(
                        product => product.Barcode == barcode,
                        cancellationToken);

                if (existingBarcode is not null)
                {
                    throw new ValidationException(
                        "A product with this barcode already exists.");
                }
            }

            // Validate image URLs
            if (request.Images.Any(image =>
                string.IsNullOrWhiteSpace(image.ImageUrl)))
            {
                throw new ValidationException(
                    "Image URL cannot be empty.");
            }

            // Create product
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
                IsActive = true,

                Category = category,
                Brand = brand,

                Images = [.. request.Images.Select(image =>
                    new ProductImage
                    {
                        ImageUrl = image.ImageUrl.Trim(),
                        AltText = image.AltText,
                        IsPrimary = image.IsPrimary,
                        SortOrder = image.SortOrder
                    })]
            };

            // Save product and related images
            await productRepository.AddAsync(
                product,
                cancellationToken);

            await productRepository.SaveChangesAsync(
                cancellationToken);

            return mapper.Map<ProductResponse>(product);
        }
    }
}
