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

            var product = await productRepository.GetByIdAsync(
                command.Id,
                cancellationToken);

            if (product is null || !product.IsActive)
                return null;

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

            var duplicateSku =
                await productRepository.FindFirstOrDefaultAsync(
                    x => x.SKU == sku && x.Id != command.Id,
                    cancellationToken);

            if (duplicateSku is not null)
            {
                throw new ValidationException(
                    "A product with this SKU already exists.");
            }

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

            product.SKU = sku;
            product.Barcode = barcode;
            product.Name = request.Name.Trim();
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.CostPrice = request.CostPrice;
            product.SellingPrice = request.SellingPrice;
            product.MinimumStockLevel = request.MinimumStockLevel;

            // Keep the navigation properties in sync for response mapping.
            product.Category = category;
            product.Brand = brand;

            RepositoryUpdate(productRepository, product);

            await productRepository.SaveChangesAsync(cancellationToken);

            return mapper.Map<ProductResponse>(product);
        }

        private static void RepositoryUpdate(
            IGenericRepository<Product> repository,
            Product product)
        {
            repository.Update(product);
        }
    }
}
