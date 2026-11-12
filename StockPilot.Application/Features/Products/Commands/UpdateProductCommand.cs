
using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Products.Commands
{
    public record UpdateProductCommand(
        int Id,
        UpdateProductRequest Request) : IRequest<ProductResponse?>;

    public class UpdateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<Brand> brandRepository,
        IMapper mapper,
        IValidator<UpdateProductRequest> validator)
        : IRequestHandler<UpdateProductCommand, ProductResponse?>
    {
        public async Task<ProductResponse?> Handle(
            UpdateProductCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            // Load product with existing images (tracked)
            var products =
                await productRepository.GetAllWithIncludesTrackedAsync(
                    cancellationToken,
                    product => product.Images);

            var product = products.FirstOrDefault(
                product => product.Id == command.Id &&
                           product.IsActive);

            if (product is null)
            {
                return null;
            }

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
            var duplicateSku =
                await productRepository.FindFirstOrDefaultAsync(
                    x => x.SKU == sku && x.Id != command.Id,
                    cancellationToken);

            if (duplicateSku is not null)
            {
                throw new ValidationException(
                    "A product with this SKU already exists.");
            }

            // Check duplicate barcode
            if (barcode is not null)
            {
                var duplicateBarcode =
                    await productRepository.FindFirstOrDefaultAsync(
                        x => x.Barcode == barcode && x.Id != command.Id,
                        cancellationToken);

                if (duplicateBarcode is not null)
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

            // Update product details
            product.SKU = sku;
            product.Barcode = barcode;
            product.Name = request.Name.Trim();
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.CostPrice = request.CostPrice;
            product.SellingPrice = request.SellingPrice;
            product.MinimumStockLevel = request.MinimumStockLevel;

            product.Category = category;
            product.Brand = brand;

            // Replace existing images
            product.Images.Clear();

            foreach (var image in request.Images)
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = image.ImageUrl.Trim(),
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary,
                    SortOrder = image.SortOrder
                });
            }

            productRepository.Update(product);

            await productRepository.SaveChangesAsync(
                cancellationToken);

            return mapper.Map<ProductResponse>(product);
        }
    }
}
